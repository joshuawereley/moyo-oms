using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Domain.Allocation;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence;

using static System.FormattableString;

namespace Moyo.Oms.Seeder;

public sealed class OrderSeeder
{
    private const int BatchSize = 500;

    public static async Task SeedAsync(OmsDbContext context, SeedOptions options, CancellationToken cancellationToken = default)
    {
        var random = new Random(1234);

        int externalSystemId = await context.ExternalSystems.Select(system => system.Id).FirstAsync(cancellationToken);
        List<int> productIds = await context.ProductReferences.Select(product => product.Id).ToListAsync(cancellationToken);

        Dictionary<int, List<VendorProduct>> candidatesByProduct = (await context.VendorProducts
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .GroupBy(listing => listing.ProductReferenceId)
            .ToDictionary(group => group.Key, group => group.ToList());

        int seeded = 0;
        while (seeded < options.Orders)
        {
            int batchCount = Math.Min(BatchSize, options.Orders - seeded);
            var plans = new List<OrderPlan>(batchCount);

            for (int index = 0; index < batchCount; index++)
            {
                OrderPlan plan = BuildOrder(random, externalSystemId, productIds, candidatesByProduct, seeded + index);
                context.CustomerOrders.Add(plan.Order);
                plans.Add(plan);
            }

            await context.SaveChangesAsync(cancellationToken);

            foreach (OrderPlan plan in plans)
            {
                if (plan.VendorId is int vendorId)
                {
                    context.OrderAllocations.Add(new OrderAllocation(new OrderAllocationDecision
                    {
                        OrderId = plan.Order.Id,
                        VendorId = vendorId,
                        DecisionReason = plan.DecisionReason!,
                    }));
                }

                foreach ((OrderStatus previous, OrderStatus next) in plan.Transitions)
                {
                    context.OrderStatusHistories.Add(new OrderStatusHistory(new OrderStatusChange
                    {
                        OrderId = plan.Order.Id,
                        ChangedByVendorUserId = null,
                        PreviousStatus = previous,
                        NewStatus = next,
                    }));
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();

            seeded += batchCount;
        }
    }

    private static OrderPlan BuildOrder(
        Random random,
        int externalSystemId,
        List<int> productIds,
        Dictionary<int, List<VendorProduct>> candidatesByProduct,
        int sequence)
    {
        var incomingEvent = new IncomingOrderEvent(new IncomingOrderEventDetails
        {
            ExternalSystemId = externalSystemId,
            ServiceBusMessageId = Invariant($"seed-{sequence}-{Guid.NewGuid():N}"),
            ClientPortalOrderId = Invariant($"CPO-{sequence:D7}"),
        });

        var order = new CustomerOrder(incomingEvent, new CustomerOrderDetails
        {
            ClientPortalOrderId = Invariant($"CPO-{sequence:D7}"),
            ClientReference = Invariant($"REF-{sequence:D7}"),
        });

        int lineCount = random.Next(1, 4);
        var requirements = new List<LineRequirement>(lineCount);
        var chosen = new HashSet<int>();
        for (int line = 0; line < lineCount; line++)
        {
            int productId = productIds[random.Next(productIds.Count)];
            if (!chosen.Add(productId))
            {
                continue;
            }

            int quantity = random.Next(1, 6);
            order.AddLineItem(new OrderLineItemDetails
            {
                ProductReferenceId = productId,
                Quantity = quantity,
                UnitPrice = 9.99m,
            });
            requirements.Add(new LineRequirement { ProductReferenceId = productId, Quantity = quantity });
        }

        var candidates = requirements
            .SelectMany(requirement => candidatesByProduct.TryGetValue(requirement.ProductReferenceId, out List<VendorProduct>? list)
                ? list
                : Enumerable.Empty<VendorProduct>())
            .ToList();
        AllocationSelection? selection = AllocationPolicy.SelectVendor(requirements, candidates);

        bool leaveReceived = random.Next(100) < 20;
        if (selection is null || leaveReceived)
        {
            return new OrderPlan(order, null, null, Array.Empty<(OrderStatus, OrderStatus)>());
        }

        var transitions = new List<(OrderStatus Previous, OrderStatus New)>();
        order.MarkAllocated();
        transitions.Add((OrderStatus.Received, OrderStatus.Allocated));

        int roll = random.Next(100);
        if (roll >= 60)
        {
            order.MarkInProgress();
            transitions.Add((OrderStatus.Allocated, OrderStatus.InProgress));
            if (roll >= 85)
            {
                order.Complete();
                transitions.Add((OrderStatus.InProgress, OrderStatus.Completed));
            }
        }

        string reason = Invariant($"Allocated to vendor {selection.VendorId} at the lowest in-stock total price of {selection.TotalPrice}.");
        return new OrderPlan(order, selection.VendorId, reason, transitions);
    }

    private sealed record OrderPlan(
        CustomerOrder Order,
        int? VendorId,
        string? DecisionReason,
        IReadOnlyList<(OrderStatus Previous, OrderStatus New)> Transitions);
}
