namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// A request to change a vendor product's selling price.
/// </summary>

public sealed record RepriceVendorProductRequest
{
    public required int VendorProductId { get; init; }
    public required decimal NewPrice { get; init; }
    public required int ChangedByVendorUserId { get; init; }
}
