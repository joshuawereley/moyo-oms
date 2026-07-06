using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the OutgoingStatusEvent entity to its database table.
/// </summary>

public sealed class OutgoingStatusEventConfiguration : IEntityTypeConfiguration<OutgoingStatusEvent>
{
    public void Configure(EntityTypeBuilder<OutgoingStatusEvent> builder)
    {
        builder.ToTable("OutgoingStatusEvents");

        builder.HasKey(outgoingEvent => outgoingEvent.Id);

        builder.Property(outgoingEvent => outgoingEvent.ServiceBusMessageId)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(outgoingEvent => outgoingEvent.ServiceBusMessageId)
            .IsUnique();

        builder.HasIndex(outgoingEvent => outgoingEvent.Status);

        builder.HasOne(outgoingEvent => outgoingEvent.OrderStatusHistory)
            .WithOne()
            .HasForeignKey<OutgoingStatusEvent>(outgoingEvent => outgoingEvent.StatusHistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(outgoingEvent => outgoingEvent.ExternalSystem)
            .WithMany()
            .HasForeignKey(outgoingEvent => outgoingEvent.ExternalSystemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
