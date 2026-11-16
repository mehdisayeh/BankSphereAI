using BankSphere.SharedKernel;

namespace BankSphere.CreditAssessment.Domain;

public sealed record CreditApplicationSubmitted(Guid ApplicationId, Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
