namespace BankSphere.Security;

public static class BankingPermissions
{
    // Existing role/policy vocabulary is also the permission vocabulary; no alternate roles.
    public const string Claim = "permission";
    public const string Scope = "banking";
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(new[]
    {
        "banking-reader", "banking-operator", "fraud-reviewer", "credit-officer", "compliance-officer", "simulator"
    });
}
