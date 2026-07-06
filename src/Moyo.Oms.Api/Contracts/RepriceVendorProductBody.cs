namespace Moyo.Oms.Api.Contracts;

/// <summary>
/// The request body for repricing a vendor product.
/// </summary>

public sealed record RepriceVendorProductBody
{
    public required decimal NewPrice { get; init; }
    public required int ChangedByVendorUserId { get; init; }
}
