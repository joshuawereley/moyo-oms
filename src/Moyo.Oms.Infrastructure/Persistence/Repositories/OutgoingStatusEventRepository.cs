using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the outbox (outgoing status events).
/// </summary>

public sealed class OutgoingStatusEventRepository : IOutgoingStatusEventRepository
{
    private readonly OmsDbContext _context;

    public OutgoingStatusEventRepository(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OutgoingStatusEvent>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.OutgoingStatusEvents
            .Include(statusEvent => statusEvent.OrderStatusHistory)
                .ThenInclude(history => history.CustomerOrder)
            .Where(statusEvent => statusEvent.Status == PublishStatus.Pending)
            .OrderBy(statusEvent => statusEvent.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public void Add(OutgoingStatusEvent statusEvent)
    {
        _context.OutgoingStatusEvents.Add(statusEvent);
    }
}
