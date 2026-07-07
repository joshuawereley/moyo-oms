using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Stores immutable order status-history records.
/// </summary>

public interface IOrderStatusHistoryRepository
{
    void Add(OrderStatusHistory statusHistory);
}
