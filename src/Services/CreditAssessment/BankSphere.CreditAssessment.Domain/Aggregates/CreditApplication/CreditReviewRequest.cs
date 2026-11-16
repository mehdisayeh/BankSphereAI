
namespace BankSphere.CreditAssessment.Domain;

public sealed record CreditReviewRequest(Guid Id, string Reviewer, string Reason, DateTimeOffset RequestedAtUtc, DateTimeOffset? CompletedAtUtc);
