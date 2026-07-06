using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the IncomingOrderEvent entity to its database table.
/// </summary>

public sealed class IncomingOrderEventConfiguration : IEntityTypeConfiguration<IncomingOrderEvent>
{
    public void Configure(EntityTypeBuilder<IncomingOrderEvent> builder)
    {
        builder.ToTable("IncomingOrderEvents");

        builder.HasKey(incomingOrderEvent => incomingOrderEvent.Id);

        builder.Property(incomingOrderEvent => incomingOrderEvent.ServiceBusMessageId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(incomingOrderEvent => incomingOrderEvent.ClientPortalOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(incomingOrderEvent => incomingOrderEvent.ServiceBusMessageId)
            .IsUnique();

        builder.HasOne(incomingOrderEvent => incomingOrderEvent.ExternalSystem)
            .WithMany()
            .HasForeignKey(incomingOrderEvent => incomingOrderEvent.ExternalSystemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
