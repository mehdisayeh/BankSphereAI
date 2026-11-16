using BankSphere.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Identity.Infrastructure;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "identity"); builder.HasKey(value => value.Id); builder.Property(value => value.Id).HasMaxLength(100); builder.Property(value => value.ClientId).HasMaxLength(100); builder.Property(value => value.SecurityStamp).HasMaxLength(100); builder.Property(value => value.RevocationReason).HasMaxLength(100); builder.HasIndex(value => new { value.UserId, value.RevokedAtUtc }); builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(value => value.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
