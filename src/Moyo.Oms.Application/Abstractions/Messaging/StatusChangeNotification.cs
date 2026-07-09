using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Application.Abstractions.Messaging;

/// <summary>
/// The data required to publish an order-status change to an external system.
/// </summary>

public sealed record StatusChangeNotification
{
    public required string MessageId { get; init; }
    public required string ClientPortalOrderId { get; init; }
    public required OrderStatus Status { get; init; }
    public string? StatusNote { get; init; }
    public required DateTimeOffset ChangedAt { get; init; }
}
