namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data captured when a new-order message is received.
/// </summary>

public sealed record IncomingOrderEventDetails
{
    public required int ExternalSystemId { get; init; }
    public required string ServiceBusMessageId { get; init; }
    public required string ClientPortalOrderId { get; init; }
}
