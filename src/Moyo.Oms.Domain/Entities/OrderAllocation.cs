using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A record of allocating an order to a vendor.
/// </summary>

public class OrderAllocation : Entity
{
    private OrderAllocation() { }

    public OrderAllocation(OrderAllocationDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(decision.OrderId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(decision.VendorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(decision.DecisionReason);

        OrderId = decision.OrderId;
        VendorId = decision.VendorId;
        DecisionReason = decision.DecisionReason;
        Status = AllocationStatus.Allocated;
        AllocatedAt = DateTimeOffset.UtcNow;
    }

    public int OrderId { get; private set; }

    public CustomerOrder CustomerOrder { get; private set; } = null!;

    public int VendorId { get; private set; }

    public Vendor Vendor { get; private set; } = null!;

    public AllocationStatus Status { get; private set; }

    public string DecisionReason { get; private set; } = null!;

    public DateTimeOffset AllocatedAt { get; private set; }

    public void Reject(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Status = AllocationStatus.Rejected;
        DecisionReason = reason;
    }
}
