using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Infrastructure.Persistence;

using Testcontainers.MsSql;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

/// <summary>
/// Starts a real SQL Server in Docker once for the whole test collection,
/// applies the EF Core migrations, and hands out fresh DbContext instances.
/// </summary>

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public OmsDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<OmsDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options);
}
