using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the VendorUser entity to its database table.
/// </summary>

public sealed class VendorUserConfiguration : IEntityTypeConfiguration<VendorUser>
{
    public void Configure(EntityTypeBuilder<VendorUser> builder)
    {
        builder.ToTable("VendorUsers");

        builder.HasKey(vendorUser => vendorUser.Id);

        builder.Property(vendorUser => vendorUser.AzureAdUserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(vendorUser => vendorUser.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(vendorUser => vendorUser.EmailAddress)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(vendorUser => vendorUser.AzureAdUserId)
            .IsUnique();

        builder.HasOne(vendorUser => vendorUser.Vendor)
            .WithMany()
            .HasForeignKey(vendorUser => vendorUser.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
