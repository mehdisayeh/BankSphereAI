namespace BankSphere.Identity.Domain;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711", Justification = "Permission is the authorization domain concept, not a code access security permission.")]
public sealed class Permission
{
    private Permission() { }
    public Permission(string name) { ArgumentException.ThrowIfNullOrWhiteSpace(name); Name = name; }
    public string Name { get; private set; } = "";
}
