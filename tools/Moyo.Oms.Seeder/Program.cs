using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moyo.Oms.Infrastructure;
using Moyo.Oms.Infrastructure.Persistence;
using Moyo.Oms.Seeder;

var builder = Host.CreateApplicationBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("OmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

builder.Services.AddInfrastructure(connectionString);

SeedOptions options =
    builder.Configuration.GetSection(SeedOptions.SectionName).Get<SeedOptions>() ?? new SeedOptions();

using var host = builder.Build();

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
OmsDbContext context = scope.ServiceProvider.GetRequiredService<OmsDbContext>();

Console.WriteLine("Resetting database...");
await ReferenceDataSeeder.ResetAsync(context);

Console.WriteLine($"Seeding {options.Vendors} vendors and {options.Products} products...");
await ReferenceDataSeeder.SeedAsync(context, options);

Console.WriteLine("Seeding vendor products...");
await VendorProductSeeder.SeedAsync(context);

Console.WriteLine("Reference and vendor-product data seeded.");
