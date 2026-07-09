using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Stores outbound status events awaiting publication (the outbox).
/// </summary>

public interface IOutgoingStatusEventRepository
{
    Task<IReadOnlyList<OutgoingStatusEvent>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);

    void Add(OutgoingStatusEvent statusEvent);
}
