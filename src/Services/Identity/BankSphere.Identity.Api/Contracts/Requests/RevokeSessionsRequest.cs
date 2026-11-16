namespace BankSphere.Identity.Api.Contracts.Requests;

public sealed record RevokeSessionsRequest(string? SessionId = null)
{
    public override string ToString() => "RevokeSessionsRequest [REDACTED]";
}
