
namespace BankSphere.CreditAssessment.Domain;

public sealed record CreditDecisionHistory(Guid Id, CreditApplicationStatus Status, string Rationale, string EvidenceReference, string Actor, DateTimeOffset AtUtc);
