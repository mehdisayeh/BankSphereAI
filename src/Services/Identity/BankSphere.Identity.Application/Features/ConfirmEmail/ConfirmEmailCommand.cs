namespace BankSphere.Identity.Application;

public sealed record ConfirmEmailCommand(string Email, string Token)
{
    public override string ToString() => "ConfirmEmailCommand [REDACTED]";
}
