using BankSphere.SharedKernel;

namespace BankSphere.Identity.Application;

public static class ForgotPasswordCommandValidator
{
    public static void Validate(ForgotPasswordCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Guard.Against(!System.Net.Mail.MailAddress.TryCreate(value.Email, out _) || value.Email.Length > 254, "identity.request", "A valid email address is required.");
    }
}
