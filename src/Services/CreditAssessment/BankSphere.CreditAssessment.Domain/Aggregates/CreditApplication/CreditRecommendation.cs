
namespace BankSphere.CreditAssessment.Domain;

public sealed record CreditRecommendation(Guid Id, CreditDecisionRecommendation Recommendation, string Rationale, string EvidenceReference, string Actor, DateTimeOffset AtUtc);
