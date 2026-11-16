
namespace BankSphere.Transactions.Domain;

public sealed record TransactionRiskSignal(Guid Id, string Code, string Severity, string EvidenceReference, DateTimeOffset DetectedAtUtc);
