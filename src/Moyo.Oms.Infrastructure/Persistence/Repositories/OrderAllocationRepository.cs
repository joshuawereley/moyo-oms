using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the order allocation repository.
/// </summary>

public sealed class OrderAllocationRepository : IOrderAllocationRepository
{
    private readonly OmsDbContext _context;

    public OrderAllocationRepository(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderAllocations
            .AnyAsync(allocation => allocation.OrderId == orderId, cancellationToken);
    }

    public void Add(OrderAllocation allocation)
    {
        _context.OrderAllocations.Add(allocation);
    }
}
