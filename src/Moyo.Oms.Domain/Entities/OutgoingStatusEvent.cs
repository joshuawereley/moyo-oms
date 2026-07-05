using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// An outbox record that publishes an order-status change to an external system.
/// </summary>

public class OutgoingStatusEvent : Entity
{
    private OutgoingStatusEvent() {}

    public OutgoingStatusEvent(OutgoingStatusEventDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.ExternalSystemId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.StatusHistoryId);

        ExternalSystemId = details.ExternalSystemId;
        StatusHistoryId = details.StatusHistoryId;
        ServiceBusMessageId = Guid.NewGuid().ToString();
        CreatedAt = DateTimeOffset.UtcNow;
        Status = PublishStatus.Pending;
    }

    public int ExternalSystemId { get; private set; }

    public ExternalSystem ExternalSystem { get; private set; } = null!;

    public int StatusHistoryId { get; private set; }

    public OrderStatusHistory OrderStatusHistory { get; private set; } = null!;

    public string ServiceBusMessageId { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public PublishStatus Status { get; private set; }

    public void MarkPublished()
    {
        Status = PublishStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed() => Status = PublishStatus.Failed;
}
