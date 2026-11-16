using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Domain;

public sealed record HumanReviewRequested(Guid CaseId, string Reviewer, Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
