namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Drains pending outbox rows by publishing them to external systems.
/// </summary>

public interface IOutboxPublisher
{
    Task<int> PublishPendingAsync(int batchSize, CancellationToken cancellationToken = default);
}
