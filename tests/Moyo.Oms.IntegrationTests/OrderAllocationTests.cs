using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Orders;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence;
using Moyo.Oms.Infrastructure.Persistence.Repositories;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

[Collection("Database")]
public sealed class OrderAllocationTests
{
    private readonly SqlServerFixture _fixture;

    public OrderAllocationTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Allocate_WithEligibleVendors_AllocatesToCheapestAndNotifies()
    {
        var (orderId, productReferenceId, _) = await SeedReceivedOrderAsync(quantity: 2);
        await SeedVendorProductAsync(productReferenceId, price: 10m, stock: 5, vendorName: "Expensive");
        int cheapVendor = await SeedVendorProductAsync(productReferenceId, price: 8m, stock: 5, vendorName: "Cheap");

        AllocationOutcome outcome;
        await using (var context = _fixture.CreateContext())
        {
            outcome = await BuildService(context).AllocateAsync(orderId);
        }

        outcome.Should().Be(AllocationOutcome.Allocated);

        await using (var context = _fixture.CreateContext())
        {
            var order = await context.CustomerOrders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Allocated);

            var allocation = await context.OrderAllocations.SingleAsync(a => a.OrderId == orderId);
            allocation.VendorId.Should().Be(cheapVendor);
            allocation.Status.Should().Be(AllocationStatus.Allocated);

            var history = await context.OrderStatusHistories.SingleAsync(h => h.OrderId == orderId);
            history.PreviousStatus.Should().Be(OrderStatus.Received);
            history.NewStatus.Should().Be(OrderStatus.Allocated);

            var outbox = await context.OutgoingStatusEvents.SingleAsync(e => e.StatusHistoryId == history.Id);
            outbox.Status.Should().Be(PublishStatus.Pending);
        }
    }

    [Fact]
    public async Task Allocate_WhenAlreadyAllocated_ReturnsSkipped()
    {
        var (orderId, productReferenceId, _) = await SeedReceivedOrderAsync(quantity: 1);
        await SeedVendorProductAsync(productReferenceId, price: 8m, stock: 5, vendorName: "Only");

        await using (var context = _fixture.CreateContext())
        {
            (await BuildService(context).AllocateAsync(orderId)).Should().Be(AllocationOutcome.Allocated);
        }

        await using (var context = _fixture.CreateContext())
        {
            (await BuildService(context).AllocateAsync(orderId)).Should().Be(AllocationOutcome.Skipped);
        }

        await using (var context = _fixture.CreateContext())
        {
            (await context.OrderAllocations.CountAsync(a => a.OrderId == orderId)).Should().Be(1);
        }
    }

    [Fact]
    public async Task Allocate_WithNoInStockVendor_ReturnsNoEligibleVendorAndLeavesReceived()
    {
        var (orderId, productReferenceId, _) = await SeedReceivedOrderAsync(quantity: 1);
        await SeedVendorProductAsync(productReferenceId, price: 8m, stock: 0, vendorName: "OutOfStock");

        AllocationOutcome outcome;
        await using (var context = _fixture.CreateContext())
        {
            outcome = await BuildService(context).AllocateAsync(orderId);
        }

        outcome.Should().Be(AllocationOutcome.NoEligibleVendor);

        await using (var context = _fixture.CreateContext())
        {
            var order = await context.CustomerOrders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Received);
            (await context.OrderAllocations.CountAsync(a => a.OrderId == orderId)).Should().Be(0);
            (await context.OrderStatusHistories.CountAsync(h => h.OrderId == orderId)).Should().Be(0);
        }
    }

    private static AllocationService BuildService(OmsDbContext context) =>
        new(
            new OrderRepository(context),
            new VendorProductRepository(context),
            new OrderAllocationRepository(context),
            new OrderStatusHistoryRepository(context),
            new OutgoingStatusEventRepository(context),
            context);

    private async Task<(int OrderId, int ProductReferenceId, int ExternalSystemId)> SeedReceivedOrderAsync(int quantity)
    {
        await using var context = _fixture.CreateContext();

        var externalSystem = new ExternalSystem("Client Portal", IntegrationType.ServiceBus);
        context.ExternalSystems.Add(externalSystem);
        await context.SaveChangesAsync();

        var productReference = new ProductReference(new ProductReferenceDetails
        {
            ExternalSystemId = externalSystem.Id,
            PmsProductId = Guid.NewGuid().ToString(),
            ProductName = "Widget",
            ProductCategory = "General",
        });
        context.ProductReferences.Add(productReference);
        await context.SaveChangesAsync();

        var incomingEvent = new IncomingOrderEvent(new IncomingOrderEventDetails
        {
            ExternalSystemId = externalSystem.Id,
            ServiceBusMessageId = Guid.NewGuid().ToString(),
            ClientPortalOrderId = "CPO-ALLOC",
        });
        var order = new CustomerOrder(incomingEvent, new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-ALLOC",
            ClientReference = "REF-ALLOC",
        });
        order.AddLineItem(new OrderLineItemDetails
        {
            ProductReferenceId = productReference.Id,
            Quantity = quantity,
            UnitPrice = 9m,
        });
        context.CustomerOrders.Add(order);
        await context.SaveChangesAsync();

        return (order.Id, productReference.Id, externalSystem.Id);
    }

    private async Task<int> SeedVendorProductAsync(int productReferenceId, decimal price, int stock, string vendorName)
    {
        await using var context = _fixture.CreateContext();

        var vendor = new Vendor(vendorName);
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();

        var vendorProduct = new VendorProduct(new VendorProductListing
        {
            VendorId = vendor.Id,
            ProductReferenceId = productReferenceId,
            SellingPrice = price,
            StockQuantity = stock,
        });
        context.VendorProducts.Add(vendorProduct);
        await context.SaveChangesAsync();

        return vendor.Id;
    }
}
