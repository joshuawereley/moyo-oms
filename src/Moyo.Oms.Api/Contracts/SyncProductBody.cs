namespace Moyo.Oms.Api.Contracts;

/// <summary>
/// The request body for synchronising a product from the PMS.
/// </summary>

public sealed record SyncProductBody
{
    public required int ExternalSystemId { get; init; }
    public required string PmsProductId { get; init; }
}
