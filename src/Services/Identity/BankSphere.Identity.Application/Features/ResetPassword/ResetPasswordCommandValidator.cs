using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class ResetPasswordCommandValidator
{
    public static void Validate(ResetPasswordCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Against(!System.Net.Mail.MailAddress.TryCreate(value.Email, out _) || value.Email.Length > 254, "identity.request", "A valid email address is required.");
        Guard.Against(string.IsNullOrWhiteSpace(value.NewPassword) || value.NewPassword.Length > 256, "identity.request", "A password of at most 256 characters is required.");
        Guard.Text(value.Token, "Token", 8192);
    }
}
