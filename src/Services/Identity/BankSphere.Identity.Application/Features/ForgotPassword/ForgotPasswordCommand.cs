namespace BankSphere.Identity.Application;

public sealed record ForgotPasswordCommand(string Email)
{
    public override string ToString() => "ForgotPasswordCommand [REDACTED]";
}
