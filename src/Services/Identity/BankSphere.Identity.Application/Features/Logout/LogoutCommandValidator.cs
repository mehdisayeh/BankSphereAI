using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class LogoutCommandValidator
{
    public static void Validate(LogoutCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Id(value.UserId, "User");
        Guard.Text(value.SessionId, "Session", 100);
    }
}
