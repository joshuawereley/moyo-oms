using FluentAssertions;

using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence.Repositories;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

[Collection("Database")]
public sealed class VendorProductPersistenceTests
{
    private readonly SqlServerFixture _fixture;

    public VendorProductPersistenceTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_ThenGetById_RoundTripsThroughRealSqlServer()
    {
        int vendorProductId;

        await using (var context = _fixture.CreateContext())
        {
            var externalSystem = new ExternalSystem("PMS", IntegrationType.Rest);
            context.ExternalSystems.Add(externalSystem);
            var vendor = new Vendor("Acme Supplies");
            context.Vendors.Add(vendor);
            await context.SaveChangesAsync();

            var productReference = new ProductReference(new ProductReferenceDetails
            {
                ExternalSystemId = externalSystem.Id,
                PmsProductId = "PMS-001",
                ProductName = "Widget",
                ProductCategory = "General",
            });
            context.ProductReferences.Add(productReference);
            await context.SaveChangesAsync();

            var vendorProduct = new VendorProduct(new VendorProductListing
            {
                VendorId = vendor.Id,
                ProductReferenceId = productReference.Id,
                SellingPrice = 19.99m,
                StockQuantity = 5,
            });
            new VendorProductRepository(context).Add(vendorProduct);
            await context.SaveChangesAsync();

            vendorProductId = vendorProduct.Id;
        }

        await using (var context = _fixture.CreateContext())
        {
            var reloaded = await new VendorProductRepository(context).GetByIdAsync(vendorProductId);

            reloaded.Should().NotBeNull();
            reloaded!.SellingPrice.Should().Be(19.99m);
            reloaded.StockQuantity.Should().Be(5);
            reloaded.Availability.Should().Be(AvailabilityStatus.InStock);
        }
    }
}
