namespace Moyo.Oms.Domain.Enums;

/// <summary>
/// Lifecycle state of a customer order with the OMS.
/// </summary>

public enum OrderStatus
{
    Received = 1,
    Allocated = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}
