namespace BankSphere.Identity.Application;

public sealed record RegisterCommand(string Email, string Password, string DisplayName)
{
    public override string ToString() => "RegisterCommand [REDACTED]";
}
