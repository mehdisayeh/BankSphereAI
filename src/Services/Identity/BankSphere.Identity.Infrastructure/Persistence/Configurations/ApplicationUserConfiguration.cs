using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Identity.Infrastructure;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users", "identity");
        builder.Property(user => user.DisplayName).HasMaxLength(100).IsRequired();
        builder.HasIndex(user => user.NormalizedEmail).IsUnique().HasDatabaseName("EmailIndex").HasFilter("[NormalizedEmail] IS NOT NULL");
        builder.HasIndex(user => user.CustomerId).HasFilter("[CustomerId] IS NOT NULL");
    }
}
