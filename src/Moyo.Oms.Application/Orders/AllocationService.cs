using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Allocation;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

using static System.FormattableString;

namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Allocates a received order to a vendor and notifies the client of the change.
/// </summary>

public sealed class AllocationService : IAllocationService
{
    private readonly IOrderRepository _orders;
    private readonly IVendorProductRepository _vendorProducts;
    private readonly IOrderAllocationRepository _allocations;
    private readonly IOrderStatusHistoryRepository _statusHistory;
    private readonly IOutgoingStatusEventRepository _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public AllocationService(
        IOrderRepository orders,
        IVendorProductRepository vendorProducts,
        IOrderAllocationRepository allocations,
        IOrderStatusHistoryRepository statusHistory,
        IOutgoingStatusEventRepository outbox,
        IUnitOfWork unitOfWork)
    {
        _orders = orders;
        _vendorProducts = vendorProducts;
        _allocations = allocations;
        _statusHistory = statusHistory;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<AllocationOutcome> AllocateAsync(int orderId, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(orderId);

        if (await _allocations.ExistsForOrderAsync(orderId, cancellationToken))
        {
            return AllocationOutcome.Skipped;
        }

        CustomerOrder order =
            await _orders.GetForAllocationAsync(orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(CustomerOrder), orderId);

        if (order.Status != OrderStatus.Received)
        {
            return AllocationOutcome.Skipped;
        }

        List<LineRequirement> requirements = order.LineItems
            .Select(lineItem => new LineRequirement
            {
                ProductReferenceId = lineItem.ProductReferenceId,
                Quantity = lineItem.Quantity,
            })
            .ToList();

        IReadOnlyList<VendorProduct> candidates = await LoadCandidatesAsync(requirements, cancellationToken);

        AllocationSelection? selection = AllocationPolicy.SelectVendor(requirements, candidates);

        if (selection is null)
        {
            return AllocationOutcome.NoEligibleVendor;
        }

        string decisionReason = Invariant(
            $"Allocated to vendor {selection.VendorId} at the lowest in-stock total price of {selection.TotalPrice}.");

        OrderAllocation allocation = new(new OrderAllocationDecision
        {
            OrderId = order.Id,
            VendorId = selection.VendorId,
            DecisionReason = decisionReason,
        });

        OrderStatus previousStatus = order.Status;
        order.MarkAllocated();

        await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                _allocations.Add(allocation);

                OrderStatusHistory history = new(new OrderStatusChange
                {
                    OrderId = order.Id,
                    ChangedByVendorUserId = null,
                    PreviousStatus = previousStatus,
                    NewStatus = order.Status,
                    StatusNote = decisionReason,
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

        return AllocationOutcome.Allocated;
    }

    private async Task<IReadOnlyList<VendorProduct>> LoadCandidatesAsync(
        IReadOnlyCollection<LineRequirement> requirements,
        CancellationToken cancellationToken)
    {
        List<VendorProduct> candidates = new();

        foreach (int productReferenceId in requirements.Select(requirement => requirement.ProductReferenceId).Distinct())
        {
            candidates.AddRange(await _vendorProducts.GetByProductReferenceAsync(productReferenceId, cancellationToken));
        }

        return candidates;
    }
}
