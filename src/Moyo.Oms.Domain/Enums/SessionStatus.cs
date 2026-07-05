namespace Moyo.Oms.Domain.Enums;

/// <summary>
/// Lifecycle state of a vendor user's login session.
/// </summary>

public enum SessionStatus
{
    Active = 1,
    LoggedOut = 2,
    Expired = 3,
    Revoked = 4
}
