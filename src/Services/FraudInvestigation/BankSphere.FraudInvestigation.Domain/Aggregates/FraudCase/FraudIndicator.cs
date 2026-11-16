
namespace BankSphere.FraudInvestigation.Domain;

public sealed record FraudIndicator(Guid Id, string Type, FraudSeverity Severity, string EvidenceReference, DateTimeOffset DetectedAtUtc);
