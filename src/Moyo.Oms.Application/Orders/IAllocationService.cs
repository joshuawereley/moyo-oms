namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Allocates a received order to the vendor selected by the allocation policy.
/// </summary>

public interface IAllocationService
{
    Task<AllocationOutcome> AllocateAsync(int orderId, CancellationToken cancellationToken = default);
}
