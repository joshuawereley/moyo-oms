using FluentAssertions;

using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence.Queries;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

[Collection("Database")]
public sealed class ReadQueriesTests
{
    private readonly SqlServerFixture _fixture;

    public ReadQueriesTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VendorProductQueries_ReturnsOnlyTheVendorsProductsPaged()
    {
        var seed = await SeedAsync();

        await using var context = _fixture.CreateContext();
        var result = await new VendorProductQueries(context).GetPageByVendorAsync(seed.VendorAId, 1, 20);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].ProductName.Should().Be("Widget");
        result.Items[0].Availability.Should().Be("InStock");
    }

    [Fact]
    public async Task OrderQueries_ReturnsOnlyOrdersAllocatedToTheVendor()
    {
        var seed = await SeedAsync();

        await using var context = _fixture.CreateContext();
        var result = await new OrderQueries(context).GetPageByVendorAsync(seed.VendorAId, 1, 20);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(item => item.Id == seed.OrderId);
        result.Items[0].Status.Should().Be("Allocated");

        var others = await new OrderQueries(context).GetPageByVendorAsync(seed.VendorBId, 1, 20);
        others.Items.Should().NotContain(item => item.Id == seed.OrderId);
    }

    [Fact]
    public async Task OrderDetail_ReturnsLinesAndHistoryForOwningVendorAndNullForOthers()
    {
        var seed = await SeedAsync();

        await using var context = _fixture.CreateContext();

        var detail = await new OrderQueries(context).GetDetailForVendorAsync(seed.OrderId, seed.VendorAId);
        detail.Should().NotBeNull();
        detail!.Lines.Should().ContainSingle();
        detail.Lines[0].ProductName.Should().Be("Widget");
        detail.History.Should().Contain(entry => entry.NewStatus == "Allocated");

        var forOtherVendor = await new OrderQueries(context).GetDetailForVendorAsync(seed.OrderId, seed.VendorBId);
        forOtherVendor.Should().BeNull();
    }

    private async Task<SeedResult> SeedAsync()
    {
        await using var context = _fixture.CreateContext();

        var externalSystem = new ExternalSystem("Client Portal", IntegrationType.ServiceBus);
        context.ExternalSystems.Add(externalSystem);
        var vendorA = new Vendor("Vendor A");
        var vendorB = new Vendor("Vendor B");
        context.Vendors.AddRange(vendorA, vendorB);
        await context.SaveChangesAsync();

        var productReference = new ProductReference(new ProductReferenceDetails
        {
            ExternalSystemId = externalSystem.Id,
            PmsProductId = Guid.NewGuid().ToString(),
            ProductName = "Widget",
            ProductCategory = "Electronics",
        });
        context.ProductReferences.Add(productReference);
        await context.SaveChangesAsync();

        context.VendorProducts.Add(new VendorProduct(new VendorProductListing
        {
            VendorId = vendorA.Id,
            ProductReferenceId = productReference.Id,
            SellingPrice = 10m,
            StockQuantity = 5,
        }));
        context.VendorProducts.Add(new VendorProduct(new VendorProductListing
        {
            VendorId = vendorB.Id,
            ProductReferenceId = productReference.Id,
            SellingPrice = 12m,
            StockQuantity = 5,
        }));

        var incomingEvent = new IncomingOrderEvent(new IncomingOrderEventDetails
        {
            ExternalSystemId = externalSystem.Id,
            ServiceBusMessageId = Guid.NewGuid().ToString(),
            ClientPortalOrderId = "CPO-A",
        });
        var order = new CustomerOrder(incomingEvent, new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-A",
            ClientReference = "REF-A",
        });
        order.AddLineItem(new OrderLineItemDetails
        {
            ProductReferenceId = productReference.Id,
            Quantity = 2,
            UnitPrice = 9m,
        });
        order.MarkAllocated();
        context.CustomerOrders.Add(order);
        await context.SaveChangesAsync();

        context.OrderAllocations.Add(new OrderAllocation(new OrderAllocationDecision
        {
            OrderId = order.Id,
            VendorId = vendorA.Id,
            DecisionReason = "test allocation",
        }));
        context.OrderStatusHistories.Add(new OrderStatusHistory(new OrderStatusChange
        {
            OrderId = order.Id,
            ChangedByVendorUserId = null,
            PreviousStatus = OrderStatus.Received,
            NewStatus = OrderStatus.Allocated,
        }));
        await context.SaveChangesAsync();

        return new SeedResult(vendorA.Id, vendorB.Id, order.Id);
    }

    private sealed record SeedResult(int VendorAId, int VendorBId, int OrderId);
}
