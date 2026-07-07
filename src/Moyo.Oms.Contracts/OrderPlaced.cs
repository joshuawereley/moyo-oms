namespace Moyo.Oms.Contracts;

/// <summary>
/// A new order placed on the Client Portal, published to the orders.new topic.
/// </summary>

public sealed record OrderPlaced
{
    public required string ClientPortalOrderId { get; init; }
    public required string ClientReference { get; init; }
    public required IReadOnlyList<OrderPlacedLine> Lines { get; init; }
}

/// <summary>
/// A single product line on a placed order.
/// </summary>

public sealed record OrderPlacedLine
{
    public required string PmsProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
