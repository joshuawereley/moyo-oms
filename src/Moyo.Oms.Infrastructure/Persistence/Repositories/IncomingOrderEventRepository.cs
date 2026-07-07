using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the inbound-event repository.
/// </summary>

public sealed class IncomingOrderEventRepository : IIncomingOrderEventRepository
{
    private readonly OmsDbContext _context;

    public IncomingOrderEventRepository(OmsDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(string serviceBusMessageId, CancellationToken cancellationToken = default)
    {
        return _context.IncomingOrderEvents
            .AnyAsync(incomingEvent => incomingEvent.ServiceBusMessageId == serviceBusMessageId, cancellationToken);
    }

    public void Add(IncomingOrderEvent incomingOrderEvent)
    {
        _context.IncomingOrderEvents.Add(incomingOrderEvent);
    }
}
