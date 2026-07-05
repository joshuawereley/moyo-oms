using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A vendor that maintains price and stock and manages allocated orders.
/// </summary>

public class Vendor : Entity
{
    private Vendor()
    {
    }

    public Vendor(string vendorName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vendorName);

        VendorName = vendorName;
        Status = VendorStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string VendorName { get; private set; } = null!;

    public VendorStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Suspend() => Transition(VendorStatus.Suspended);

    public void Reactivate() => Transition(VendorStatus.Active);

    public void Deactivate() => Transition(VendorStatus.Inactive);

    private void Transition(VendorStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
