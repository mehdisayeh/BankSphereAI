namespace BankSphere.Identity.Application;

public sealed record RevokeSessionsCommand(Guid UserId, string? SessionId = null)
{
    public override string ToString() => "RevokeSessionsCommand [REDACTED]";
}
