namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Owns the customer-order lifecycle (C4: Order Management Service).
/// </summary>

public interface IOrderService
{
    Task<int> CreateOrderAsync(CreateOrderRequest request,
            CancellationToken cancellationToken = default);
}
