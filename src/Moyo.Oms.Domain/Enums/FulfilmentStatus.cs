namespace Moyo.Oms.Domain.Enums;

/// <summary>
/// Whether the goods for a customer order have been fulfilled.
/// </summary>

public enum FulfilmentStatus
{
    Pending = 1,
    PartiallyFulfilled = 2,
    Fulfilled = 3,
    Unfulfilled = 4
}
