using BankSphere.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Identity.Infrastructure;

public sealed class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("SecurityAuditLogs", "identity"); builder.HasKey(value => value.Id); builder.Property(value => value.Action).HasMaxLength(100); builder.Property(value => value.CorrelationId).HasMaxLength(100); builder.HasIndex(value => new { value.UserId, value.OccurredAtUtc });
    }
}
