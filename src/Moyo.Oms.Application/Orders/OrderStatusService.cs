using FluentValidation;

using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Applies a vendor's order-status change and enqueues the outbound notification.
/// </summary>

public sealed class OrderStatusService : IOrderStatusService
{
    private readonly IValidator<ChangeOrderStatusRequest> _validator;
    private readonly IOrderRepository _orders;
    private readonly IOrderStatusHistoryRepository _statusHistory;
    private readonly IOutgoingStatusEventRepository _outbox;
    private readonly ICurrentVendorUserProvider _currentVendorUser;
    private readonly IUnitOfWork _unitOfWork;

    public OrderStatusService(
        IValidator<ChangeOrderStatusRequest> validator,
        IOrderRepository orders,
        IOrderStatusHistoryRepository statusHistory,
        IOutgoingStatusEventRepository outbox,
        ICurrentVendorUserProvider currentVendorUser,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _orders = orders;
        _statusHistory = statusHistory;
        _outbox = outbox;
        _currentVendorUser = currentVendorUser;
        _unitOfWork = unitOfWork;
    }

    public async Task ChangeStatusAsync(
        ChangeOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        int changedByVendorUserId =
            await _currentVendorUser.GetVendorUserIdAsync(cancellationToken);

        CustomerOrder order =
            await _orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(CustomerOrder), request.OrderId);

        OrderStatus previousStatus = order.Status;

        Transition(order, request.TargetStatus);

        await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                OrderStatusHistory history = new(new OrderStatusChange
                {
                    OrderId = order.Id,
                    ChangedByVendorUserId = changedByVendorUserId,
                    PreviousStatus = previousStatus,
                    NewStatus = order.Status,
                    StatusNote = request.StatusNote,
                });

                _statusHistory.Add(history);
                await _unitOfWork.SaveChangesAsync(ct);

                OutgoingStatusEvent statusEvent = new(new OutgoingStatusEventDetails
                {
                    ExternalSystemId = order.IncomingOrderEvent.ExternalSystemId,
                    StatusHistoryId = history.Id,
                });

                _outbox.Add(statusEvent);
                await _unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);
    }

    private static void Transition(CustomerOrder order, OrderStatus targetStatus)
    {
        switch (targetStatus)
        {
            case OrderStatus.InProgress
                when order.Status is OrderStatus.Received or OrderStatus.Allocated:
                order.MarkInProgress();
                break;

            case OrderStatus.Completed when order.Status is OrderStatus.InProgress:
                order.Complete();
                break;

            case OrderStatus.Cancelled
                when order.Status is OrderStatus.Received or OrderStatus.Allocated or OrderStatus.InProgress:
                order.Cancel();
                break;

            default:
                throw new ConflictException(
                    $"Order {order.Id} cannot move from {order.Status} to {targetStatus}.");
        }
    }
}
