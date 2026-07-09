using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Stores order-to-vendor allocations.
/// </summary>

public interface IOrderAllocationRepository
{
    Task<bool> ExistsForOrderAsync(int orderId, CancellationToken cancellationToken = default);

    void Add(OrderAllocation allocation);
}
