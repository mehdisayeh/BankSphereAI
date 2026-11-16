namespace BankSphere.Identity.Application;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
{
    public override string ToString() => "ChangePasswordCommand [REDACTED]";
}
