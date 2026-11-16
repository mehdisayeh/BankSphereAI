namespace BankSphere.Identity.Application;

public sealed record LoginCommand(string Email, string Password, string? AuthenticatorCode = null)
{
    public override string ToString() => "LoginCommand [REDACTED]";
}
