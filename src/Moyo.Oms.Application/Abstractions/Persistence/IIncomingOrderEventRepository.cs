using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Loads and stores inbound integration events.
/// </summary>

public interface IIncomingOrderEventRepository
{
    Task<bool> ExistsAsync(string serviceBusMessageId,
            CancellationToken cancellationToken = default);

    void Add(IncomingOrderEvent incomingOrderEvent);
}
