using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Api.Authorization;

/// <summary>
/// Authorization policy names and the app-role claim they check.
/// </summary>

public static class AuthorizationPolicies
{
    public const string RoleClaimType = "roles";

    public const string VendorAdministrator = nameof(VendorRole.VendorAdministrator);
}
