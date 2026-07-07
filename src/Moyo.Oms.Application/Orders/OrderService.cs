using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Handles the customer-order lifecycle.
/// </summary>

public sealed class OrderService : IOrderService
{
    private readonly IIncomingOrderEventRepository _events;
    private readonly IOrderRepository _orders;
    private readonly IProductReferenceRepository _productReferences;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IIncomingOrderEventRepository events,
        IOrderRepository orders,
        IProductReferenceRepository productReferences,
        IUnitOfWork unitOfWork)
    {
        _events = events;
        _orders = orders;
        _productReferences = productReferences;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await _events.ExistsAsync(request.ServiceBusMessageId, cancellationToken))
        {
            return;
        }

        IncomingOrderEvent incomingEvent = new(new IncomingOrderEventDetails
        {
            ExternalSystemId = request.ExternalSystemId,
            ServiceBusMessageId = request.ServiceBusMessageId,
            ClientPortalOrderId = request.ClientPortalOrderId,
        });

        CustomerOrder order = new(incomingEvent, new CustomerOrderDetails
        {
            ClientPortalOrderId = request.ClientPortalOrderId,
            ClientReference = request.ClientReference,
        });

        foreach (CreateOrderLine line in request.Lines)
        {
            ProductReference product =
                await _productReferences.GetByPmsProductIdAsync(
                    request.ExternalSystemId,
                    line.PmsProductId,
                    cancellationToken)
                ?? throw new NotFoundException(nameof(ProductReference), line.PmsProductId);

            order.AddLineItem(new OrderLineItemDetails
            {
                ProductReferenceId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
            });
        }

        incomingEvent.MarkProcessed();

        _events.Add(incomingEvent);
        _orders.Add(order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
