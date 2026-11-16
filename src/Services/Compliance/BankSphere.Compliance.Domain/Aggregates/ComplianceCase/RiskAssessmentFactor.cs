
namespace BankSphere.Compliance.Domain;

public sealed record RiskAssessmentFactor(Guid Id, RiskClassification Classification, string Code, string Rationale, string Assessor, DateTimeOffset AtUtc);
