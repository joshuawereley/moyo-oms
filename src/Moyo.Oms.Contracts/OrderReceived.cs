namespace Moyo.Oms.Contracts;

/// <summary>
/// Signals that an order has been received and is ready for vendor allocation.
/// Published by the intake worker; consumed by the Allocation Function.
/// </summary>

public sealed record OrderReceived
{
    public required int OrderId { get; init; }
}
