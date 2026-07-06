using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the ExternalSystem entity to its database table.
/// </summary>

public sealed class ExternalSystemConfiguration : IEntityTypeConfiguration<ExternalSystem>
{
    public void Configure(EntityTypeBuilder<ExternalSystem> builder)
    {
        builder.ToTable("ExternalSystems");

        builder.HasKey(externalSystem => externalSystem.Id);

        builder.Property(externalSystem => externalSystem.SystemName)
            .IsRequired()
            .HasMaxLength(200);
    }
}
