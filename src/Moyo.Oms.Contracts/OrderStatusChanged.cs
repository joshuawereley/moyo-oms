namespace Moyo.Oms.Contracts;

/// <summary>
/// An order-status change published to external systems on the orders.status topic.
/// </summary>

public sealed record OrderStatusChanged
{
    public required string ClientPortalOrderId { get; init; }
    public required string Status { get; init; }
    public string? StatusNote { get; init; }
    public required DateTimeOffset ChangedAt { get; init; }
}
