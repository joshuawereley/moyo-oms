using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the VendorProduct entity to its database table.
/// </summary>

public sealed class VendorProductConfiguration : IEntityTypeConfiguration<VendorProduct>
{
    public void Configure(EntityTypeBuilder<VendorProduct> builder)
    {
        builder.ToTable("VendorProducts");

        builder.HasKey(vendorProduct => vendorProduct.Id);

        builder.Property(vendorProduct => vendorProduct.SellingPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder
            .HasIndex(vendorProduct => new
            {
                vendorProduct.VendorId,
                vendorProduct.ProductReferenceId,
            })
            .IsUnique();

        builder.HasOne(vendorProduct => vendorProduct.Vendor)
            .WithMany()
            .HasForeignKey(vendorProduct => vendorProduct.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(vendorProduct => vendorProduct.ProductReference)
            .WithMany()
            .HasForeignKey(vendorProduct => vendorProduct.ProductReferenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
