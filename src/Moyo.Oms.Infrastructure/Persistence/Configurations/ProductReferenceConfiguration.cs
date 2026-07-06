using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the ProductReference entity to its database table.
/// </summary>

public sealed class ProductReferenceConfiguration : IEntityTypeConfiguration<ProductReference>
{
    public void Configure(EntityTypeBuilder<ProductReference> builder)
    {
        builder.ToTable("ProductReferences");

        builder.HasKey(productReference => productReference.Id);

        builder.Property(productReference => productReference.PmsProductId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(productReference => productReference.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(productReference => productReference.ProductCategory)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .HasIndex(productReference => new
            {
                productReference.ExternalSystemId,
                productReference.PmsProductId,
            })
            .IsUnique();

        builder.HasOne(productReference => productReference.ExternalSystem)
            .WithMany()
            .HasForeignKey(productReference => productReference.ExternalSystemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
