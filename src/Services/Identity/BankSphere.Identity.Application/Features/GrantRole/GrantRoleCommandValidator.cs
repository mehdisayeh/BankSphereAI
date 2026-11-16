using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class GrantRoleCommandValidator
{
    public static void Validate(GrantRoleCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Text(value.Email, "Email", 254); Guard.Text(value.Role, "Role", 100);
    }
}
