namespace BankSphere.Identity.Application;

public sealed record LogoutCommand(Guid UserId, string SessionId)
{
    public override string ToString() => "LogoutCommand [REDACTED]";
}
