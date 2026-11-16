
namespace BankSphere.FraudInvestigation.Domain;

public sealed record HumanReviewRequest(Guid Id, string Reviewer, string Reason, DateTimeOffset RequestedAtUtc, string? Decision, DateTimeOffset? DecidedAtUtc);
