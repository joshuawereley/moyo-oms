using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Infrastructure.Persistence;

namespace Moyo.Oms.Seeder;

public sealed class VendorProductSeeder
{
    private const int BatchSize = 1000;

    public static async Task SeedAsync(OmsDbContext context, CancellationToken cancellationToken = default)
    {
        var random = new Random(42);

        List<int> vendorIds = await context.Vendors.Select(vendor => vendor.Id).ToListAsync(cancellationToken);
        List<int> productIds = await context.ProductReferences.Select(product => product.Id).ToListAsync(cancellationToken);

        var listings = new List<VendorProduct>(vendorIds.Count * productIds.Count);

        foreach (int productId in productIds)
        {
            decimal basePrice = 10m + random.Next(0, 490);

            foreach (int vendorId in vendorIds)
            {
                decimal price = Math.Max(1m, basePrice + random.Next(-5, 6));
                int stock = random.Next(0, 101);

                listings.Add(new VendorProduct(new VendorProductListing
                {
                    VendorId = vendorId,
                    ProductReferenceId = productId,
                    SellingPrice = price,
                    StockQuantity = stock,
                }));
            }
        }

        for (int start = 0; start < listings.Count; start += BatchSize)
        {
            context.VendorProducts.AddRange(listings.Skip(start).Take(BatchSize));
            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();
        }
    }
}
