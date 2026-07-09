using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the order repository.
/// </summary>

public sealed class OrderRepository : IOrderRepository
{
    private readonly OmsDbContext _context;

    public OrderRepository(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerOrder?> GetByIdAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerOrders
            .Include(order => order.IncomingOrderEvent)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public void Add(CustomerOrder order)
    {
        _context.CustomerOrders.Add(order);
    }

    public async Task<int?> GetIdByServiceBusMessageIdAsync(
        string serviceBusMessageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerOrders
            .Where(order => order.IncomingOrderEvent.ServiceBusMessageId == serviceBusMessageId)
            .Select(order => (int?)order.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CustomerOrder?> GetForAllocationAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerOrders
            .Include(order => order.IncomingOrderEvent)
            .Include(order => order.LineItems)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }
}
