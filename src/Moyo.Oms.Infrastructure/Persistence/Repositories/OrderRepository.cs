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

    public void Add(CustomerOrder order)
    {
        _context.CustomerOrders.Add(order);
    }
}
