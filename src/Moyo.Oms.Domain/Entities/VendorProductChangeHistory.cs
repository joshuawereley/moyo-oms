using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// An immutable audit record of a change to a vendor produc's price or stock.
/// </summary>

public class VendorProductChangeHistory : Entity
{
    private VendorProductChangeHistory()
    {
    }

    public VendorProductChangeHistory(VendorProductChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(change.VendorProductId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(change.ChangedByVendorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(change.PreviousValue);
        ArgumentException.ThrowIfNullOrWhiteSpace(change.NewValue);

        VendorProductId = change.VendorProductId;
        ChangedByVendorUserId = change.ChangedByVendorUserId;
        ChangeType = change.ChangeType;
        PreviousValue = change.PreviousValue;
        NewValue = change.NewValue;
        ChangedAt = DateTimeOffset.UtcNow;
    }

    public int VendorProductId { get; private set; }

    public VendorProduct VendorProduct { get; private set; } = null!;

    public int ChangedByVendorUserId { get; private set; }

    public VendorUser ChangedByVendorUser { get; private set; } = null!;

    public ChangeType ChangeType { get; private set; }

    public string PreviousValue { get; private set; } = null!;

    public string NewValue { get; private set; } = null!;

    public DateTimeOffset ChangedAt { get; private set; }
}
