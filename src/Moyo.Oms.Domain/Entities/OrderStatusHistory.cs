using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// An immutable audit record of a change to an order's status.
/// </summary>

public class OrderStatusHistory : Entity
{
    private OrderStatusHistory()
    {
    }

    public OrderStatusHistory(OrderStatusChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(change.OrderId);

        if (change.ChangedByVendorUserId is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(change),
                "ChangedByVendorUserId must be positive when provided.");
        }

        OrderId = change.OrderId;
        ChangedByVendorUserId = change.ChangedByVendorUserId;
        PreviousStatus = change.PreviousStatus;
        NewStatus = change.NewStatus;
        StatusNote = change.StatusNote;
        ChangedAt = DateTimeOffset.UtcNow;
    }

    public int OrderId { get; private set; }

    public CustomerOrder CustomerOrder { get; private set; } = null!;

    public int? ChangedByVendorUserId { get; private set; }

    public VendorUser? ChangedByVendorUser { get; private set; }

    public OrderStatus PreviousStatus { get; private set; }

    public OrderStatus NewStatus { get; private set; }

    public string? StatusNote { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }
}
