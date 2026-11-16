using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class LoginCommandValidator
{
    public static void Validate(LoginCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Against(!System.Net.Mail.MailAddress.TryCreate(value.Email, out _) || value.Email.Length > 254, "identity.request", "A valid email address is required.");
        Guard.Against(string.IsNullOrWhiteSpace(value.Password) || value.Password.Length > 256, "identity.request", "A password of at most 256 characters is required.");
    }
}
