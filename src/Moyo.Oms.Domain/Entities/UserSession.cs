using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A vendor user's sign-in session and its lifecycle.
/// </summary>

public class UserSession : Entity
{
    private UserSession()
    {
    }

    public UserSession(UserSessionStart start)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(start.VendorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(start.TokenId);

        VendorUserId = start.VendorUserId;
        TokenId = start.TokenId;
        ExpiresAt = start.ExpiresAt;
        LoginAt = DateTimeOffset.UtcNow;
        Status = SessionStatus.Active;
    }

    public int VendorUserId { get; private set; }

    public VendorUser VendorUser { get; private set; } = null!;

    public string TokenId { get; private set; } = null!;

    public DateTimeOffset LoginAt { get; private set; }

    public DateTimeOffset? LogoutAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public SessionStatus Status { get; private set; }

    public void LogOut()
    {
        LogoutAt = DateTimeOffset.UtcNow;
        Status = SessionStatus.LoggedOut;
    }

    public void Expire() => Status = SessionStatus.Expired;

    public void Revoke() => Status = SessionStatus.Revoked;
}
