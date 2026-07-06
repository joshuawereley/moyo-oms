using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Api.Authorization;

/// <summary>
/// Authorization policy names.
/// </summary>
public static class AuthorizationPolicies
{
    public const string VendorAdministrator = nameof(VendorRole.VendorAdministrator);
}
