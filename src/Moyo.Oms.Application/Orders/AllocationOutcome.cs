namespace Moyo.Oms.Application.Orders;

/// <summary>
/// The result of attempting to allocate an order to a vendor.
/// </summary>

public enum AllocationOutcome
{
    Allocated,
    Skipped,
    NoEligibleVendor,
}
