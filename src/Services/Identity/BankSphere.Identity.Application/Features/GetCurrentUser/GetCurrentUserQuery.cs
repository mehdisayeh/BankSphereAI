namespace BankSphere.Identity.Application;

public sealed record GetCurrentUserQuery(Guid UserId)
{
    public override string ToString() => "GetCurrentUserQuery [REDACTED]";
}
