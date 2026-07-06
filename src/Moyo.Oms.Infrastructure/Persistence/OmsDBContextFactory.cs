using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Moyo.Oms.Infrastructure.Persistence;

/// <summary>
/// Creates the OmsDbContext for EF Core design-time tooling (migrations).
/// </summary>

public sealed class OmsDbContextFactory : IDesignTimeDbContextFactory<OmsDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=localhost;Database=MoyoOms;Trusted_Connection=True;TrustServerCertificate=True;";

    public OmsDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("OMS_DB_CONNECTION") ?? DesignTimeConnectionString;

        DbContextOptionsBuilder<OmsDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(connectionString);

        return new OmsDbContext(optionsBuilder.Options);
    }
}
