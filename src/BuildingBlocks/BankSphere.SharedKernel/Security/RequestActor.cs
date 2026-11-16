

namespace BankSphere.SharedKernel;

public sealed record RequestActor(string Subject, IReadOnlySet<string> Roles)
{
    public void Require(string role)
    {
        Guard.Text(Subject, "Authenticated subject");
        Guard.Against(!Roles.Contains(role), "authorization.role_required", $"The {role} role is required.", FailureKind.Forbidden);
    }
}
