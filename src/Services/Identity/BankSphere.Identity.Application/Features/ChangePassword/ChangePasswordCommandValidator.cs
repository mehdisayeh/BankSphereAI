using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class ChangePasswordCommandValidator
{
    public static void Validate(ChangePasswordCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Against(string.IsNullOrWhiteSpace(value.CurrentPassword) || value.CurrentPassword.Length > 256, "identity.request", "A password of at most 256 characters is required.");
        Guard.Against(string.IsNullOrWhiteSpace(value.NewPassword) || value.NewPassword.Length > 256, "identity.request", "A password of at most 256 characters is required.");
        Guard.Id(value.UserId, "User");
    }
}
