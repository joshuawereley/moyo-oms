using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the vendor product repository.
/// </summary>

public sealed class VendorProductRepository : IVendorProductRepository
{
    private readonly OmsDbContext _context;

    public VendorProductRepository(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<VendorProduct?> GetByIdAsync(
        int vendorProductId,
        CancellationToken cancellationToken = default)
    {
        return await _context.VendorProducts.FindAsync([vendorProductId], cancellationToken);
    }

    public async Task<IReadOnlyList<VendorProduct>> GetByVendorAsync(
        int vendorId,
        CancellationToken cancellationToken = default)
    {
        return await _context.VendorProducts
            .Where(vendorProduct => vendorProduct.VendorId == vendorId)
            .ToListAsync(cancellationToken);
    }

    public void Add(VendorProduct vendorProduct)
    {
        _context.VendorProducts.Add(vendorProduct);
    }
}
