namespace Moyo.Oms.Domain.Allocation;

/// <summary>
/// The vendor chosen to fulfil an order and the resulting total price.
/// </summary>

public sealed record AllocationSelection
{
    public required int VendorId { get; init; }
    public required decimal TotalPrice { get; init; }
}
