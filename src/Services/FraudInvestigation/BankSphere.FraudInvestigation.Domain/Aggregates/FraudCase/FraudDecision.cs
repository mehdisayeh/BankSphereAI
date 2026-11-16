
namespace BankSphere.FraudInvestigation.Domain;

public sealed record FraudDecision(Guid Id, FraudDecisionClassification Classification, string Rationale, string Reviewer, string EvidenceReference, DateTimeOffset DecidedAtUtc);
