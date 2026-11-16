using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class ConfirmEmailCommandValidator
{
    public static void Validate(ConfirmEmailCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Against(!System.Net.Mail.MailAddress.TryCreate(value.Email, out _) || value.Email.Length > 254, "identity.request", "A valid email address is required.");
        Guard.Text(value.Token, "Token", 8192);
    }
}
