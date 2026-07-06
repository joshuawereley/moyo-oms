using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the vendor user repository.
/// </summary>

public sealed class VendorUserRepository : IVendorUserRepository
{
    private readonly OmsDbContext _context;

    public VendorUserRepository(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<VendorUser?> GetByAzureAdUserIdAsync(
        string azureAdUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.VendorUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                vendorUser => vendorUser.AzureAdUserId == azureAdUserId,
                cancellationToken);
    }
}
