using FluentAssertions;

using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

using Xunit;

namespace Moyo.Oms.Domain.Tests;

public class CustomerOrderTests
{
    [Fact]
    public void Constructor_WithValidDetails_StartsInReceivedPendingState()
    {
        var order = new CustomerOrder(Event(), new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-1",
            ClientReference = "REF-1",
        });

        order.ClientPortalOrderId.Should().Be("CPO-1");
        order.ClientReference.Should().Be("REF-1");
        order.Status.Should().Be(OrderStatus.Received);
        order.FulfilmentStatus.Should().Be(FulfilmentStatus.Pending);
        order.OrderTotal.Should().Be(0m);
        order.AllocatedAt.Should().BeNull();
        order.LineItems.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullIncomingEvent_Throws()
    {
        var details = new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-1",
            ClientReference = "REF-1",
        };

        var act = () => new CustomerOrder(null!, details);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullDetails_Throws()
    {
        var act = () => new CustomerOrder(Event(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithBlankClientPortalOrderId_Throws(string clientPortalOrderId)
    {
        var act = () => new CustomerOrder(Event(), new CustomerOrderDetails
        {
            ClientPortalOrderId = clientPortalOrderId,
            ClientReference = "REF-1",
        });

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithBlankClientReference_Throws(string clientReference)
    {
        var act = () => new CustomerOrder(Event(), new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-1",
            ClientReference = clientReference,
        });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddLineItem_WithNull_Throws()
    {
        var order = Order();

        var act = () => order.AddLineItem(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddLineItem_AddsLineAndRecalculatesTotal()
    {
        var order = Order();

        order.AddLineItem(Line(quantity: 2, unitPrice: 10m));

        order.LineItems.Should().HaveCount(1);
        order.OrderTotal.Should().Be(20m);
    }

    [Fact]
    public void AddLineItem_MultipleLines_SumsOrderTotal()
    {
        var order = Order();

        order.AddLineItem(Line(productReferenceId: 1, quantity: 2, unitPrice: 10m));
        order.AddLineItem(Line(productReferenceId: 2, quantity: 3, unitPrice: 5m));

        order.LineItems.Should().HaveCount(2);
        order.OrderTotal.Should().Be(35m);
    }

    [Fact]
    public void MarkAllocated_SetsAllocatedStatusAndTimestamp()
    {
        var order = Order();

        order.MarkAllocated();

        order.Status.Should().Be(OrderStatus.Allocated);
        order.AllocatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkInProgress_SetsInProgressStatus()
    {
        var order = Order();

        order.MarkInProgress();

        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public void Complete_SetsCompletedStatus()
    {
        var order = Order();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Cancel_SetsCancelledStatus()
    {
        var order = Order();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void UpdateFulfilment_SetsFulfilmentStatus()
    {
        var order = Order();

        order.UpdateFulfilment(FulfilmentStatus.Fulfilled);

        order.FulfilmentStatus.Should().Be(FulfilmentStatus.Fulfilled);
    }

    private static IncomingOrderEvent Event() =>
        new(new IncomingOrderEventDetails
        {
            ExternalSystemId = 1,
            ServiceBusMessageId = "sb-1",
            ClientPortalOrderId = "CPO-1",
        });

    private static CustomerOrder Order() =>
        new(Event(), new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-1",
            ClientReference = "REF-1",
        });

    private static OrderLineItemDetails Line(
        int productReferenceId = 1,
        int quantity = 1,
        decimal unitPrice = 10m) =>
        new()
        {
            ProductReferenceId = productReferenceId,
            Quantity = quantity,
            UnitPrice = unitPrice,
        };
}
