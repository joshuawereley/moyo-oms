namespace Moyo.Oms.Application.Abstractions.Identity;

/// <summary>
/// Resolves the authenticated caller to their provisioned vendor user.
/// </summary>

public interface ICurrentVendorUserProvider
{
    Task<int> GetVendorUserIdAsync(CancellationToken cancellationToken = default);
}
