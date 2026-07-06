using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Identity;

/// <summary>
/// Resolves the authenticated caller to their provisioned vendor user.
/// </summary>

public sealed class CurrentVendorUserProvider : ICurrentVendorUserProvider
{
    private readonly ICurrentUser _currentUser;
    private readonly IVendorUserRepository _vendorUsers;

    public CurrentVendorUserProvider(
        ICurrentUser currentUser,
        IVendorUserRepository vendorUsers)
    {
        _currentUser = currentUser;
        _vendorUsers = vendorUsers;
    }

    public async Task<int> GetVendorUserIdAsync(CancellationToken cancellationToken = default)
    {
        string? externalUserId = _currentUser.ExternalUserId;

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            throw new ForbiddenAccessException("The caller is not an authenticated vendor user.");
        }

        VendorUser vendorUser =
            await _vendorUsers.GetByAzureAdUserIdAsync(externalUserId, cancellationToken)
            ?? throw new ForbiddenAccessException("The caller is not a provisioned vendor user.");

        return vendorUser.Id;
    }
}
