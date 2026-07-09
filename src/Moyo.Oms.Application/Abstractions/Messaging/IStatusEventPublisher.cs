namespace Moyo.Oms.Application.Abstractions.Messaging;

/// <summary>
/// Publishes order-status changes to external systems.
/// </summary>

public interface IStatusEventPublisher
{
    Task PublishAsync(StatusChangeNotification notification, CancellationToken cancellationToken = default);
}
