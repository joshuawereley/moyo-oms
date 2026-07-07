using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the order status-history repository.
/// </summary>

public sealed class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
{
    private readonly OmsDbContext _context;

    public OrderStatusHistoryRepository(OmsDbContext context)
    {
        _context = context;
    }

    public void Add(OrderStatusHistory statusHistory)
    {
        _context.OrderStatusHistories.Add(statusHistory);
    }
}
