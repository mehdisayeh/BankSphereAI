using BankSphere.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Identity.Infrastructure;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "identity"); builder.HasKey(value => new { value.RoleId, value.PermissionName }); builder.Property(value => value.PermissionName).HasMaxLength(100); builder.HasOne<Role>().WithMany().HasForeignKey(value => value.RoleId).OnDelete(DeleteBehavior.Cascade); builder.HasOne<Permission>().WithMany().HasForeignKey(value => value.PermissionName).OnDelete(DeleteBehavior.Cascade);
    }
}
