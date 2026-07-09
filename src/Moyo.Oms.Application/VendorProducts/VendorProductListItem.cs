namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// A vendor product as shown in a vendor's product list.
/// </summary>

public sealed record VendorProductListItem
{
    public required int Id { get; init; }
    public required int ProductReferenceId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductCategory { get; init; }
    public required decimal SellingPrice { get; init; }
    public required int StockQuantity { get; init; }
    public required string Availability { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
