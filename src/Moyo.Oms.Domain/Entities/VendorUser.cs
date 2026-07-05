using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A person who signs in on behalf of a vendor via Microsoft Entra ID.
/// </summary>

public class VendorUser : Entity
{
    private VendorUser() { }

    public VendorUser(VendorUserRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(registration.VendorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.AzureAdUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.FullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.EmailAddress);

        VendorId = registration.VendorId;
        AzureAdUserId = registration.AzureAdUserId;
        FullName = registration.FullName;
        EmailAddress = registration.EmailAddress;
        Role = registration.Role;
        IsActive = true;
    }

    public int VendorId { get; private set; }

    public Vendor Vendor { get; private set; } = null!;

    public string AzureAdUserId { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string EmailAddress { get; private set; } = null!;

    public VendorRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public void ChangeRole(VendorRole role) => Role = role;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
