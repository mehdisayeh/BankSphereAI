namespace BankSphere.Identity.Api.Contracts.Requests;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword)
{
    public override string ToString() => "ChangePasswordRequest [REDACTED]";
}
