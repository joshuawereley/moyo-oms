using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A durable record of a new-order message received from an external system.
/// </summary>

public class IncomingOrderEvent : Entity
{
    private IncomingOrderEvent()
    {
    }

    public IncomingOrderEvent(IncomingOrderEventDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.ExternalSystemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(details.ServiceBusMessageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(details.ClientPortalOrderId);

        ExternalSystemId = details.ExternalSystemId;
        ServiceBusMessageId = details.ServiceBusMessageId;
        ClientPortalOrderId = details.ClientPortalOrderId;
        ReceivedAt = DateTimeOffset.UtcNow;
        Status = ProcessingStatus.Received;
    }

    public int ExternalSystemId { get; private set; }

    public ExternalSystem ExternalSystem { get; private set; } = null!;

    public string ServiceBusMessageId { get; private set; } = null!;

    public string ClientPortalOrderId { get; private set; } = null!;

    public DateTimeOffset ReceivedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public ProcessingStatus Status { get; private set; }

    public void BeginProcessing() => Status = ProcessingStatus.Processing;

    public void MarkProcessed() => Complete(ProcessingStatus.Processed);

    public void MarkFailed() => Complete(ProcessingStatus.Failed);

    private void Complete(ProcessingStatus status)
    {
        Status = status;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
