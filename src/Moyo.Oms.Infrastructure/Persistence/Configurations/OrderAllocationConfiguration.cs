using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the OrderAllocation entity to its database table.
/// </summary>

public sealed class OrderAllocationConfiguration : IEntityTypeConfiguration<OrderAllocation>
{
    public void Configure(EntityTypeBuilder<OrderAllocation> builder)
    {
        builder.ToTable("OrderAllocations");

        builder.HasKey(orderAllocation => orderAllocation.Id);

        builder.Property(orderAllocation => orderAllocation.DecisionReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(orderAllocation => orderAllocation.CustomerOrder)
            .WithOne()
            .HasForeignKey<OrderAllocation>(orderAllocation => orderAllocation.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(orderAllocation => orderAllocation.Vendor)
            .WithMany()
            .HasForeignKey(orderAllocation => orderAllocation.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
