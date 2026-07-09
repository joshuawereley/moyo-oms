namespace Moyo.Oms.Application.Abstractions.Products;

/// <summary>
/// A product as returned synchronously by the Product Management System.
/// </summary>

public sealed record PmsProduct
{
    public required string PmsProductId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductCategory { get; init; }
    public required bool IsActive { get; init; }
}
