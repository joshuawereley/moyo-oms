using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the UserSession entity to its database table.
/// </summary>

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(userSession => userSession.Id);

        builder.Property(userSession => userSession.TokenId)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(userSession => userSession.TokenId);

        builder.HasOne(userSession => userSession.VendorUser)
            .WithMany()
            .HasForeignKey(userSession => userSession.VendorUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
