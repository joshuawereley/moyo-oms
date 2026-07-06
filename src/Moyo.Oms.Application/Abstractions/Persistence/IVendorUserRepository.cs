using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Loads vendor users.
/// </summary>

public interface IVendorUserRepository
{
    Task<VendorUser?> GetByAzureAdUserIdAsync(
            string azureAdUserId,
            CancellationToken cancellationToken = default);
}
