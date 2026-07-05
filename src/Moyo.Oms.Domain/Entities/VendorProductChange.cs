using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data describing a single change to a vendor product.
/// </summary>

public sealed record VendorProductChange
{
    public required int VendorProductId { get; init; }
    public required int ChangedByVendorUserId { get; init; }
    public required ChangeType ChangeType { get; init; }
    public required string PreviousValue { get; init; }
    public required string NewValue { get; init; }
}
