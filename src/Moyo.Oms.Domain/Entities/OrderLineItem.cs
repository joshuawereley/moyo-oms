using Moyo.Oms.Domain.Common;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A single product line on a customer order.
/// </summary>

public class OrderLineItem : Entity
{
    private OrderLineItem() { }

    internal OrderLineItem(OrderLineItemDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.ProductReferenceId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.Quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(details.UnitPrice);

        ProductReferenceId = details.ProductReferenceId;
        Quantity = details.Quantity;
        UnitPrice = details.UnitPrice;
        LineTotal = Quantity * UnitPrice;
    }

    public int OrderId { get; private set; }

    public CustomerOrder CustomerOrder { get; private set; } = null!;

    public int ProductReferenceId { get; private set; }

    public ProductReference ProductReference { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal { get; private set; }
}
