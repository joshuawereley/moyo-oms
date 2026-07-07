using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Application.Orders;

/// <summary>
/// A request to change a customer order's status.
/// </summary>

public sealed record ChangeOrderStatusRequest
{
    public required int OrderId { get; init; }
    public required OrderStatus TargetStatus { get; init; }
    public string? StatusNote { get; init; }
}
