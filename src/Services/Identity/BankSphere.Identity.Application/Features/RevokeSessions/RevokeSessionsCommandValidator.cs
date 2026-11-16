using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class RevokeSessionsCommandValidator
{
    public static void Validate(RevokeSessionsCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Id(value.UserId, "User");
    }
}
