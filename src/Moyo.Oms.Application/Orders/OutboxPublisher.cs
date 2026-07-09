using Moyo.Oms.Application.Abstractions.Messaging;
using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Publishes pending outbox rows to external systems and marks them published.
/// </summary>

public sealed class OutboxPublisher : IOutboxPublisher
{
    private readonly IOutgoingStatusEventRepository _outbox;
    private readonly IStatusEventPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public OutboxPublisher(
        IOutgoingStatusEventRepository outbox,
        IStatusEventPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _outbox = outbox;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> PublishPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        IReadOnlyList<OutgoingStatusEvent> pending =
            await _outbox.GetPendingAsync(batchSize, cancellationToken);

        int publishedCount = 0;

        foreach (OutgoingStatusEvent statusEvent in pending)
        {
            OrderStatusHistory history = statusEvent.OrderStatusHistory;

            StatusChangeNotification notification = new()
            {
                MessageId = statusEvent.ServiceBusMessageId,
                ClientPortalOrderId = history.CustomerOrder.ClientPortalOrderId,
                Status = history.NewStatus,
                StatusNote = history.StatusNote,
                ChangedAt = history.ChangedAt,
            };

            await _publisher.PublishAsync(notification, cancellationToken);

            statusEvent.MarkPublished();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            publishedCount++;
        }

        return publishedCount;
    }
}
