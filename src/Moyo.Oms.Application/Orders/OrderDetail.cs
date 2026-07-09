namespace Moyo.Oms.Application.Orders;

/// <summary>
/// A customer order with its line items and status history.
/// </summary>

public sealed record OrderDetail
{
    public required int Id { get; init; }
    public required string ClientPortalOrderId { get; init; }
    public required string ClientReference { get; init; }
    public required string Status { get; init; }
    public required string FulfilmentStatus { get; init; }
    public required decimal OrderTotal { get; init; }
    public required DateTimeOffset ReceivedAt { get; init; }
    public DateTimeOffset? AllocatedAt { get; init; }
    public required IReadOnlyList<OrderDetailLine> Lines { get; init; }
    public required IReadOnlyList<OrderStatusHistoryItem> History { get; init; }
}

/// <summary>
/// A single line item on an order detail.
/// </summary>

public sealed record OrderDetailLine
{
    public required int ProductReferenceId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
}

/// <summary>
/// A single entry in an order's status history.
/// </summary>

public sealed record OrderStatusHistoryItem
{
    public required string PreviousStatus { get; init; }
    public required string NewStatus { get; init; }
    public string? StatusNote { get; init; }
    public required DateTimeOffset ChangedAt { get; init; }
}
