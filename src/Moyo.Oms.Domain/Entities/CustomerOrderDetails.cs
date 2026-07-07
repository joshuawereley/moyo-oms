namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data required to create a customer order.
/// </summary>

public sealed record CustomerOrderDetails
{
    public required string ClientPortalOrderId { get; init; }
    public required string ClientReference { get; init; }
}
