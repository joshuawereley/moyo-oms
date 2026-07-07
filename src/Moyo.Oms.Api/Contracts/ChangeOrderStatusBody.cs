using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Api.Contracts;

/// <summary>
/// The request body for changing a customer order's status.
/// </summary>

public sealed record ChangeOrderStatusBody
{
    public required OrderStatus TargetStatus { get; init; }
    public string? StatusNote { get; init; }
}
