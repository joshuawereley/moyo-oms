using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Application.VendorProducts;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence;
using Moyo.Oms.Infrastructure.Persistence.Repositories;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

[Collection("Database")]
public sealed class VendorProductStockTests
{
    private readonly SqlServerFixture _fixture;

    public VendorProductStockTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AdjustStock_Increase_RaisesStockFlipsAvailabilityAndAudits()
    {
        var (vendorProductId, vendorUserId) = await SeedVendorProductAsync(stock: 0);

        await using (var context = _fixture.CreateContext())
        {
            await BuildService(context, vendorUserId).AdjustStockAsync(new AdjustVendorProductStockRequest
            {
                VendorProductId = vendorProductId,
                Direction = StockAdjustmentDirection.Increase,
                Quantity = 5,
            });
        }

        await using (var context = _fixture.CreateContext())
        {
            var vendorProduct = await context.VendorProducts.FirstAsync(p => p.Id == vendorProductId);
            vendorProduct.StockQuantity.Should().Be(5);
            vendorProduct.Availability.Should().Be(AvailabilityStatus.InStock);

            var history = await context.VendorProductChangeHistories.SingleAsync(h => h.VendorProductId == vendorProductId);
            history.ChangeType.Should().Be(ChangeType.Stock);
            history.PreviousValue.Should().Be("0");
            history.NewValue.Should().Be("5");
            history.ChangedByVendorUserId.Should().Be(vendorUserId);
        }
    }

    [Fact]
    public async Task AdjustStock_Decrease_LowersStockAndAudits()
    {
        var (vendorProductId, vendorUserId) = await SeedVendorProductAsync(stock: 10);

        await using (var context = _fixture.CreateContext())
        {
            await BuildService(context, vendorUserId).AdjustStockAsync(new AdjustVendorProductStockRequest
            {
                VendorProductId = vendorProductId,
                Direction = StockAdjustmentDirection.Decrease,
                Quantity = 4,
            });
        }

        await using (var context = _fixture.CreateContext())
        {
            (await context.VendorProducts.FirstAsync(p => p.Id == vendorProductId)).StockQuantity.Should().Be(6);
            (await context.VendorProductChangeHistories.SingleAsync(h => h.VendorProductId == vendorProductId)).NewValue.Should().Be("6");
        }
    }

    [Fact]
    public async Task AdjustStock_DecreaseBelowZero_ThrowsConflictAndWritesNothing()
    {
        var (vendorProductId, vendorUserId) = await SeedVendorProductAsync(stock: 3);

        await using (var context = _fixture.CreateContext())
        {
            var act = () => BuildService(context, vendorUserId).AdjustStockAsync(new AdjustVendorProductStockRequest
            {
                VendorProductId = vendorProductId,
                Direction = StockAdjustmentDirection.Decrease,
                Quantity = 5,
            });

            await act.Should().ThrowAsync<ConflictException>();
        }

        await using (var context = _fixture.CreateContext())
        {
            (await context.VendorProducts.FirstAsync(p => p.Id == vendorProductId)).StockQuantity.Should().Be(3);
            (await context.VendorProductChangeHistories.CountAsync(h => h.VendorProductId == vendorProductId)).Should().Be(0);
        }
    }

    private static InventoryService BuildService(OmsDbContext context, int vendorUserId) =>
        new(
            new RepriceVendorProductRequestValidator(),
            new AdjustVendorProductStockRequestValidator(),
            new VendorProductRepository(context),
            new VendorProductChangeHistoryRepository(context),
            new StubCurrentVendorUser(vendorUserId),
            context);

    private async Task<(int VendorProductId, int VendorUserId)> SeedVendorProductAsync(int stock)
    {
        await using var context = _fixture.CreateContext();

        var externalSystem = new ExternalSystem("Client Portal", IntegrationType.ServiceBus);
        context.ExternalSystems.Add(externalSystem);
        var vendor = new Vendor("Acme");
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();

        var vendorUser = new VendorUser(new VendorUserRegistration
        {
            VendorId = vendor.Id,
            AzureAdUserId = Guid.NewGuid().ToString(),
            FullName = "Admin",
            EmailAddress = "admin@example.com",
            Role = VendorRole.VendorAdministrator,
        });
        context.VendorUsers.Add(vendorUser);

        var productReference = new ProductReference(new ProductReferenceDetails
        {
            ExternalSystemId = externalSystem.Id,
            PmsProductId = Guid.NewGuid().ToString(),
            ProductName = "Widget",
            ProductCategory = "General",
        });
        context.ProductReferences.Add(productReference);
        await context.SaveChangesAsync();

        var vendorProduct = new VendorProduct(new VendorProductListing
        {
            VendorId = vendor.Id,
            ProductReferenceId = productReference.Id,
            SellingPrice = 10m,
            StockQuantity = stock,
        });
        context.VendorProducts.Add(vendorProduct);
        await context.SaveChangesAsync();

        return (vendorProduct.Id, vendorUser.Id);
    }

    private sealed class StubCurrentVendorUser : ICurrentVendorUserProvider
    {
        private readonly int _vendorUserId;

        public StubCurrentVendorUser(int vendorUserId) => _vendorUserId = vendorUserId;

        public Task<int> GetVendorUserIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_vendorUserId);

        public Task<int> GetVendorIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_vendorUserId);
    }
}
