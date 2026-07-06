using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the OrderLineItem entity to its database table.
/// </summary>

public sealed class OrderLineItemConfiguration : IEntityTypeConfiguration<OrderLineItem>
{
    public void Configure(EntityTypeBuilder<OrderLineItem> builder)
    {
        builder.ToTable("OrderLineItems");

        builder.HasKey(orderLineItem => orderLineItem.Id);

        builder.Property(orderLineItem => orderLineItem.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(orderLineItem => orderLineItem.LineTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(orderLineItem => orderLineItem.ProductReference)
            .WithMany()
            .HasForeignKey(orderLineItem => orderLineItem.ProductReferenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
