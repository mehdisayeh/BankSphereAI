using BankSphere.FraudInvestigation.Domain;

namespace BankSphere.FraudInvestigation.Application;

public sealed record RecordFraudDecision(FraudDecisionClassification Classification, string Rationale, string EvidenceReference, long ExpectedVersion);
