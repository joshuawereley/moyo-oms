namespace Moyo.Oms.Application.Orders;

/// <summary>
/// A request to create a customer order from an inbound message.
/// </summary>

public sealed record CreateOrderRequest
{
    public required int ExternalSystemId { get; init; }
    public required string ServiceBusMessageId { get; init; }
    public required string ClientPortalOrderId { get; init; }
    public required IReadOnlyList<CreateOrderLine> Lines { get; init; }
}

/// <summary>
/// A single product line on a create-order request
/// </summary>

public sealed record CreateOrderLine
{
    public required string PmsProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
