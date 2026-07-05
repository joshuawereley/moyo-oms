namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data required for a vendor to list a product's price and stock.
/// </summary>

public sealed record VendorProductListing
{
    public required int VendorId { get; init; }
    public required int ProductReferenceId { get; init; }
    public required decimal SellingPrice { get; init; }
    public required int StockQuantity { get; init; }
}
