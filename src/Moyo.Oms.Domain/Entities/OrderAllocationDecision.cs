namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The decision to allocate an order to a specific vendor.
/// </summary>

public sealed record OrderAllocationDecision
{
    public required int OrderId { get; init; }
    public required int VendorId { get; init; }
    public required string DecisionReason { get; init; }
}
