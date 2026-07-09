using Moyo.Oms.Application.VendorProducts;

namespace Moyo.Oms.Api.Contracts;

/// <summary>
/// The request body for adjusting a vendor product's stock.
/// </summary>

public sealed record AdjustVendorProductStockBody
{
    public required StockAdjustmentDirection Direction { get; init; }
    public required int Quantity { get; init; }
}
