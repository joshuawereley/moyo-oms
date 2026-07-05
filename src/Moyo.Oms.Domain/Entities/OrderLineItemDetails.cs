namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data required to add a line item to an order.
/// </summary>

public sealed record OrderLineItemDetails
{
    public required int ProductReferenceId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
