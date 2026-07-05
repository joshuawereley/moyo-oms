using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data describing a single change to an order's status.
/// </summary>

public sealed record OrderStatusChange
{
    public required int OrderId { get; init; }
    public int? ChangedByVendorUserId { get; init; }
    public required OrderStatus PreviousStatus { get; init; }
    public required OrderStatus NewStatus { get; init; }
    public string? StatusNote { get; init; }
}
