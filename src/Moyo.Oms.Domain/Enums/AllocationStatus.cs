namespace Moyo.Oms.Domain.Enums;

/// <summary>
/// Outcome of allocating an order to a vendor.
/// </summary>

public enum AllocationStatus
{
    Pending = 1,
    Allocated = 2,
    Rejected = 3,
    Failed = 4
}
