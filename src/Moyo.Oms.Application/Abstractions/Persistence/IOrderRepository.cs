using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Loads and stores customer order aggregates.
/// </summary>

public interface IOrderRepository
{
    Task<CustomerOrder?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<int?> GetIdByServiceBusMessageIdAsync(string serviceBusMessageId, CancellationToken cancellationToken = default);
    void Add(CustomerOrder order);
}
