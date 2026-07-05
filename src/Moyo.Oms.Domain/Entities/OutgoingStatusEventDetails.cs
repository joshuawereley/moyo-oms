namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data required to create an outbound status event.
/// </summary>

public sealed record OutgoingStatusEventDetails
{
    public required int ExternalSystemId { get; init; }
    public required int StatusHistoryId { get; init; }
}
