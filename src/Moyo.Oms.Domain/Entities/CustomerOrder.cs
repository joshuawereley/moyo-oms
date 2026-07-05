using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// An order received from the Client Portal and managed by the OMS.
/// </summary>

public class CustomerOrder : Entity
{
    public CustomerOrder() {}

    public CustomerOrder(CustomerOrderDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.IncomingEventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(details.ClientPortalOrderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(details.ClientReference);

        IncomingEventId = details.IncomingEventId;
        ClientPortalOrderId = details.ClientPortalOrderId;
        ClientReference = details.ClientReference;
        Status = OrderStatus.Received;
        FulfilmentStatus = FulfilmentStatus.Pending;
        ReceivedAt = DateTimeOffset.UtcNow;
    }

    public int IncomingEventId { get; private set; }

    public IncomingOrderEvent IncomingOrderEvent { get; private set; } = null!;

    public string ClientPortalOrderId { get; private set; } = null!;

    public string ClientReference { get; private set; } = null!;

    public OrderStatus Status { get; private set; }

    public FulfilmentStatus FulfilmentStatus { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }

    public DateTimeOffset? AllocatedAt { get; private set; }

    public decimal OrderTotal { get; private set; }

    public void MarkAllocated()
    {
        Status = OrderStatus.Allocated;
        AllocatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkInProgress() => Status = OrderStatus.InProgress;

    public void Complete() => Status = OrderStatus.Completed;

    public void Cancel() => Status = OrderStatus.Cancelled;

    public void UpdateFulfilment(FulfilmentStatus fulfilmentStatus) =>
        FulfilmentStatus = fulfilmentStatus;
}
