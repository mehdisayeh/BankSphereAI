
namespace BankSphere.Accounts.Domain;

public sealed record AccountEligibility(bool CustomerEligible, bool ComplianceEligible, DateTimeOffset CheckedAtUtc, string EvidenceReference);
