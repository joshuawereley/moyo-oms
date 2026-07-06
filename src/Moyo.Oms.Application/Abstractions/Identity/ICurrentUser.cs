namespace Moyo.Oms.Application.Abstractions.Identity;

/// <summary>
/// Provides the identity of the authenticated caller.
/// </summary>

public interface ICurrentUser
{
    /// <summary>
    /// The caller's external identifier (Entra object id), matching
    /// <c>VendorUser.AzureAdUserId</c>; null when there is no authenticated
    /// user.
    /// </summary>
    string? ExternalUserId { get; }
}
