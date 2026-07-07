using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

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

    public void Add(OutgoingStatusEvent statusEvent)
    {
        _context.OutgoingStatusEvents.Add(statusEvent);
    }
}
