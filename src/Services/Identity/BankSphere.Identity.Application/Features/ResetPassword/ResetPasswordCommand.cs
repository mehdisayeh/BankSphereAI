namespace BankSphere.Identity.Application;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword)
{
    public override string ToString() => "ResetPasswordCommand [REDACTED]";
}
