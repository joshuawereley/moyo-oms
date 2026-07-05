namespace Moyo.Oms.Domain.Enums;

/// <summary>
/// Processing state of an inbound integration event.
/// </summary>

public enum ProcessingStatus
{
    Received = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4
}
