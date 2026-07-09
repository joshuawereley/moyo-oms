namespace Moyo.Oms.Domain.Allocation;

/// <summary>
/// A single product the order needs, with the quantity required.
/// </summary>

public sealed record LineRequirement
{
    public required int ProductReferenceId { get; init; }
    public required int Quantity { get; init; }
}
