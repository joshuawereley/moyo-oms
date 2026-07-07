using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence;

/// <summary>
/// The EF Core database session for the OMS.
/// </summary>

public sealed class OmsDbContext : DbContext, IUnitOfWork
{
    public OmsDbContext(DbContextOptions<OmsDbContext> options)
        : base(options) { }

    public DbSet<ExternalSystem> ExternalSystems => Set<ExternalSystem>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorUser> VendorUsers => Set<VendorUser>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<ProductReference> ProductReferences => Set<ProductReference>();
    public DbSet<VendorProduct> VendorProducts => Set<VendorProduct>();
    public DbSet<VendorProductChangeHistory> VendorProductChangeHistories => Set<VendorProductChangeHistory>();
    public DbSet<IncomingOrderEvent> IncomingOrderEvents => Set<IncomingOrderEvent>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<OrderAllocation> OrderAllocations => Set<OrderAllocation>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OutgoingStatusEvent> OutgoingStatusEvents => Set<OutgoingStatusEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(
            async ct =>
            {
                await using var transaction = await Database.BeginTransactionAsync(ct);

                await operation(ct);

                await transaction.CommitAsync(ct);
            },
            cancellationToken);
    }
}
