namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Handles vendor-driven changes to a customer order's status.
/// </summary>

public interface IOrderStatusService
{
    Task ChangeStatusAsync(ChangeOrderStatusRequest request, CancellationToken cancellationToken = default);
}
