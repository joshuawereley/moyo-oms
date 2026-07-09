namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// A request to adjust a vendor product's stock quantity.
/// </summary>

public sealed record AdjustVendorProductStockRequest
{
    public required int VendorProductId { get; init; }
    public required StockAdjustmentDirection Direction { get; init; }
    public required int Quantity { get; init; }
}
