
namespace BankSphere.Compliance.Domain;

public sealed record CustomerDueDiligence(Guid Id, bool Enhanced, string Reason, string SourceOfFundsAssessment, string Officer, string EvidenceReference, DateTimeOffset AtUtc);
