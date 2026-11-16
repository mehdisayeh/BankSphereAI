namespace BankSphere.Identity.Infrastructure;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711", Justification = "RolePermission is the persisted role-to-permission grant.")]
public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public string PermissionName { get; set; } = "";
}
