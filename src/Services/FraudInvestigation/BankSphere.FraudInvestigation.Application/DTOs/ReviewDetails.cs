
namespace BankSphere.FraudInvestigation.Application;

public sealed record ReviewDetails(Guid Id, string Reviewer, string Reason, DateTimeOffset RequestedAtUtc, string? Decision, DateTimeOffset? DecidedAtUtc);
