using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Queries;
using Moyo.Oms.Application.Common;
using Moyo.Oms.Application.VendorProducts;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Queries;

/// <summary>
/// EF Core read-side queries over vendor products.
/// </summary>

public sealed class VendorProductQueries : IVendorProductQueries
{
    private readonly OmsDbContext _context;

    public VendorProductQueries(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<VendorProductListItem>> GetPageByVendorAsync(
        int vendorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<VendorProduct> query = _context.VendorProducts
            .AsNoTracking()
            .Where(vendorProduct => vendorProduct.VendorId == vendorId);

        int totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(vendorProduct => vendorProduct.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(vendorProduct => new
            {
                vendorProduct.Id,
                vendorProduct.ProductReferenceId,
                vendorProduct.ProductReference.ProductName,
                vendorProduct.ProductReference.ProductCategory,
                vendorProduct.SellingPrice,
                vendorProduct.StockQuantity,
                vendorProduct.Availability,
                vendorProduct.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        List<VendorProductListItem> items = rows
            .Select(row => new VendorProductListItem
            {
                Id = row.Id,
                ProductReferenceId = row.ProductReferenceId,
                ProductName = row.ProductName,
                ProductCategory = row.ProductCategory,
                SellingPrice = row.SellingPrice,
                StockQuantity = row.StockQuantity,
                Availability = row.Availability.ToString(),
                UpdatedAt = row.UpdatedAt,
            })
            .ToList();

        return new PagedResult<VendorProductListItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
