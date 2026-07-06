using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the OrderStatusHistory entity to its database table.
/// </summary>

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");

        builder.HasKey(statusHistory => statusHistory.Id);

        builder.Property(statusHistory => statusHistory.StatusNote)
            .HasMaxLength(500);

        builder.HasIndex(statusHistory => statusHistory.OrderId);

        builder.HasOne(statusHistory => statusHistory.CustomerOrder)
            .WithMany()
            .HasForeignKey(statusHistory => statusHistory.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(statusHistory => statusHistory.ChangedByVendorUser)
            .WithMany()
            .HasForeignKey(statusHistory => statusHistory.ChangedByVendorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
