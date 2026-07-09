using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence;

namespace Moyo.Oms.Seeder;

public sealed class ReferenceDataSeeder
{
    private static readonly string[] Categories =
    {
        "Electronics", "Home", "Toys", "Apparel", "Grocery",
    };

    public static async Task ResetAsync(OmsDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public static async Task SeedAsync(OmsDbContext context, SeedOptions options, CancellationToken cancellationToken = default)
    {
        var clientPortal = new ExternalSystem("Client Portal", IntegrationType.ServiceBus);
        context.ExternalSystems.Add(clientPortal);
        await context.SaveChangesAsync(cancellationToken);

        var vendors = Enumerable.Range(1, options.Vendors)
            .Select(index => new Vendor($"Vendor {index:D2}"))
            .ToList();
        context.Vendors.AddRange(vendors);
        await context.SaveChangesAsync(cancellationToken);

        for (int index = 0; index < vendors.Count; index++)
        {
            string azureAdUserId = index switch
            {
                0 => options.PrimaryVendorAzureAdUserId ?? Guid.NewGuid().ToString(),
                1 => options.SecondaryVendorAzureAdUserId ?? Guid.NewGuid().ToString(),
                _ => Guid.NewGuid().ToString(),
            };

            context.VendorUsers.Add(new VendorUser(new VendorUserRegistration
            {
                VendorId = vendors[index].Id,
                AzureAdUserId = azureAdUserId,
                FullName = $"{vendors[index].VendorName} Administrator",
                EmailAddress = $"admin@vendor{index + 1:D2}.example.com",
                Role = VendorRole.VendorAdministrator,
            }));
        }

        await context.SaveChangesAsync(cancellationToken);

        var products = Enumerable.Range(1, options.Products)
            .Select(index => new ProductReference(new ProductReferenceDetails
            {
                ExternalSystemId = clientPortal.Id,
                PmsProductId = $"PMS-{index:D5}",
                ProductName = $"Product {index:D5}",
                ProductCategory = Categories[index % Categories.Length],
            }))
            .ToList();
        context.ProductReferences.AddRange(products);
        await context.SaveChangesAsync(cancellationToken);
    }
}
