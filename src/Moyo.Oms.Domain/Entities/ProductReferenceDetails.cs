namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data required to create a local reference to a Product Management
/// System (PMS) product.
/// </summary>

public sealed record ProductReferenceDetails
{
    public required int ExternalSystemId { get; init; }
    public required string PmsProductId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductCategory { get; init; }
}
