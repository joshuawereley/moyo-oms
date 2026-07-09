using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Products;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Application.Products;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence;
using Moyo.Oms.Infrastructure.Persistence.Repositories;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

[Collection("Database")]
public sealed class ProductSyncTests
{
    private readonly SqlServerFixture _fixture;

    public ProductSyncTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Sync_WhenPmsHasProduct_CachesItLocally()
    {
        int externalSystemId = await SeedExternalSystemAsync();
        var pms = new PmsProduct { PmsProductId = "PMS-SYNC-1", ProductName = "Synced Widget", ProductCategory = "Electronics", IsActive = true };

        await using (var context = _fixture.CreateContext())
        {
            await BuildService(context, new StubProductCatalogClient(pms)).SyncProductAsync(new SyncProductRequest
            {
                ExternalSystemId = externalSystemId,
                PmsProductId = "PMS-SYNC-1",
            });
        }

        await using (var context = _fixture.CreateContext())
        {
            var reference = await context.ProductReferences.SingleAsync(p => p.ExternalSystemId == externalSystemId && p.PmsProductId == "PMS-SYNC-1");
            reference.ProductName.Should().Be("Synced Widget");
            reference.ProductCategory.Should().Be("Electronics");
            reference.IsActive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Sync_WhenPmsHasNoProduct_Throws()
    {
        int externalSystemId = await SeedExternalSystemAsync();

        await using var context = _fixture.CreateContext();
        var act = () => BuildService(context, new StubProductCatalogClient(null)).SyncProductAsync(new SyncProductRequest
        {
            ExternalSystemId = externalSystemId,
            PmsProductId = "PMS-MISSING",
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Sync_WhenAlreadyCached_DoesNotCallPmsOrDuplicate()
    {
        int externalSystemId = await SeedExternalSystemAsync();
        await using (var context = _fixture.CreateContext())
        {
            context.ProductReferences.Add(new ProductReference(new ProductReferenceDetails
            {
                ExternalSystemId = externalSystemId,
                PmsProductId = "PMS-CACHED-1",
                ProductName = "Existing",
                ProductCategory = "Home",
            }));
            await context.SaveChangesAsync();
        }

        var stub = new StubProductCatalogClient(new PmsProduct { PmsProductId = "PMS-CACHED-1", ProductName = "Fresh", ProductCategory = "Home", IsActive = true });
        await using (var context = _fixture.CreateContext())
        {
            await BuildService(context, stub).SyncProductAsync(new SyncProductRequest { ExternalSystemId = externalSystemId, PmsProductId = "PMS-CACHED-1" });
        }

        stub.CallCount.Should().Be(0);
        await using (var context = _fixture.CreateContext())
        {
            (await context.ProductReferences.CountAsync(p => p.ExternalSystemId == externalSystemId && p.PmsProductId == "PMS-CACHED-1")).Should().Be(1);
        }
    }

    [Fact]
    public async Task Sync_WhenPmsProductInactive_CachesDeactivated()
    {
        int externalSystemId = await SeedExternalSystemAsync();
        var pms = new PmsProduct { PmsProductId = "PMS-INACTIVE-1", ProductName = "Discontinued", ProductCategory = "Toys", IsActive = false };

        await using (var context = _fixture.CreateContext())
        {
            await BuildService(context, new StubProductCatalogClient(pms)).SyncProductAsync(new SyncProductRequest { ExternalSystemId = externalSystemId, PmsProductId = "PMS-INACTIVE-1" });
        }

        await using (var context = _fixture.CreateContext())
        {
            (await context.ProductReferences.SingleAsync(p => p.ExternalSystemId == externalSystemId && p.PmsProductId == "PMS-INACTIVE-1")).IsActive.Should().BeFalse();
        }
    }

    private static ProductSyncService BuildService(OmsDbContext context, IProductCatalogClient catalog) =>
        new(new SyncProductRequestValidator(), catalog, new ProductReferenceRepository(context), context);

    private async Task<int> SeedExternalSystemAsync()
    {
        await using var context = _fixture.CreateContext();
        var externalSystem = new ExternalSystem("PMS", IntegrationType.Rest);
        context.ExternalSystems.Add(externalSystem);
        await context.SaveChangesAsync();
        return externalSystem.Id;
    }

    private sealed class StubProductCatalogClient : IProductCatalogClient
    {
        private readonly PmsProduct? _product;

        public StubProductCatalogClient(PmsProduct? product) => _product = product;

        public int CallCount { get; private set; }

        public Task<PmsProduct?> GetProductAsync(string pmsProductId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_product);
        }
    }
}
