using System.Net.Mail;

namespace BankSphere.SharedKernel;

public sealed record EmailAddress
{
    public EmailAddress(string value)
    {
        Value = Guard.Text(value, "Email", 254);
        Guard.Against(!MailAddress.TryCreate(Value, out var address) || address.Address != Value, "contact.email", "Invalid email address.");
    }
    public string Value { get; }
}
