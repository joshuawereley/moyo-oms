using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the CustomerOrder aggregate root to its database table.
/// </summary>

public sealed class CustomerOrderConfiguration : IEntityTypeConfiguration<CustomerOrder>
{
    public void Configure(EntityTypeBuilder<CustomerOrder> builder)
    {
        builder.ToTable("CustomerOrders");

        builder.HasKey(customerOrder => customerOrder.Id);

        builder.Property(customerOrder => customerOrder.ClientPortalOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(customerOrder => customerOrder.ClientReference)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(customerOrder => customerOrder.OrderTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(customerOrder => customerOrder.IncomingOrderEvent)
            .WithOne()
            .HasForeignKey<CustomerOrder>(customerOrder => customerOrder.IncomingEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(customerOrder => customerOrder.LineItems)
            .WithOne(orderLineItem => orderLineItem.CustomerOrder)
            .HasForeignKey(orderLineItem => orderLineItem.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(customerOrder => customerOrder.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
