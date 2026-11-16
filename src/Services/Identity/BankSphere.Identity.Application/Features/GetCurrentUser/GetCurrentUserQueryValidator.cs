using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class GetCurrentUserQueryValidator
{
    public static void Validate(GetCurrentUserQuery value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Id(value.UserId, "User");
    }
}
