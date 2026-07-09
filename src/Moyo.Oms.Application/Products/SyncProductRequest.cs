namespace Moyo.Oms.Application.Products;

/// <summary>
/// A request to synchronise a product from the Product Management System into the local cache.
/// </summary>

public sealed record SyncProductRequest
{
    public required int ExternalSystemId { get; init; }
    public required string PmsProductId { get; init; }
}
