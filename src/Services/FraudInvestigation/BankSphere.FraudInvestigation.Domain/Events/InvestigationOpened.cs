using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Domain;

public sealed record InvestigationOpened(Guid CaseId, Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
