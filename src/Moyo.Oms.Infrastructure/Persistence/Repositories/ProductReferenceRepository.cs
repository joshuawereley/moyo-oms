using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the product-reference repository.
/// </summary>

public sealed class ProductReferenceRepository : IProductReferenceRepository
{
    private readonly OmsDbContext _context;

    public ProductReferenceRepository(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<ProductReference?> GetByPmsProductIdAsync(
        int externalSystemId,
        string pmsProductId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductReferences
            .AsNoTracking()
            .FirstOrDefaultAsync(
                product => product.ExternalSystemId == externalSystemId
                    && product.PmsProductId == pmsProductId,
                cancellationToken);
    }

    public void Add(ProductReference productReference)
    {
        _context.ProductReferences.Add(productReference);
    }
}
