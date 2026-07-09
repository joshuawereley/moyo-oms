namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// Vendor price and stock use cases.
/// </summary>

public interface IInventoryService
{
    Task RepriceAsync(RepriceVendorProductRequest request,
            CancellationToken cancellationToken = default);

    Task AdjustStockAsync(AdjustVendorProductStockRequest request,
            CancellationToken cancellationToken = default);
}
