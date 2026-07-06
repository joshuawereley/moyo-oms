using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the vendor product change-history repository.
/// </summary>

public sealed class VendorProductChangeHistoryRepository : IVendorProductChangeHistoryRepository
{
    private readonly OmsDbContext _context;

    public VendorProductChangeHistoryRepository(OmsDbContext context)
    {
        _context = context;
    }

    public void Add(VendorProductChangeHistory change)
    {
        _context.VendorProductChangeHistories.Add(change);
    }
}
