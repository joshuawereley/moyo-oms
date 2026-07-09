namespace Moyo.Oms.Application.Orders;

/// <summary>
/// A customer order as shown in a vendor's order list.
/// </summary>

public sealed record OrderListItem
{
    public required int Id { get; init; }
    public required string ClientPortalOrderId { get; init; }
    public required string ClientReference { get; init; }
    public required string Status { get; init; }
    public required string FulfilmentStatus { get; init; }
    public required decimal OrderTotal { get; init; }
    public required DateTimeOffset ReceivedAt { get; init; }
    public DateTimeOffset? AllocatedAt { get; init; }
}
