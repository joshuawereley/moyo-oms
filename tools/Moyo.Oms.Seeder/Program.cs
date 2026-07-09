using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moyo.Oms.Infrastructure;
using Moyo.Oms.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("OmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

builder.Services.AddInfrastructure(connectionString);

using var host = builder.Build();

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
OmsDbContext context = scope.ServiceProvider.GetRequiredService<OmsDbContext>();

await context.Database.MigrateAsync();

Console.WriteLine("Database is up to date. Seeding logic is added in the next change.");
