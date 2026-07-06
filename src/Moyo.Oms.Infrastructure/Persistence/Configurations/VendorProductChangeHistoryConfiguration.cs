using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the VendorProductChangeHistory entity to its database table.
/// </summary>

public sealed class VendorProductChangeHistoryConfiguration
    : IEntityTypeConfiguration<VendorProductChangeHistory>
{
    public void Configure(EntityTypeBuilder<VendorProductChangeHistory> builder)
    {
        builder.ToTable("VendorProductChangeHistories");

        builder.HasKey(change => change.Id);

        builder.Property(change => change.PreviousValue)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(change => change.NewValue)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(change => change.VendorProductId);

        builder.HasOne(change => change.VendorProduct)
            .WithMany()
            .HasForeignKey(change => change.VendorProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(change => change.ChangedByVendorUser)
            .WithMany()
            .HasForeignKey(change => change.ChangedByVendorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
