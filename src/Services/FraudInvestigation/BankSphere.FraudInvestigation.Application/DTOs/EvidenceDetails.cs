
namespace BankSphere.FraudInvestigation.Application;

public sealed record EvidenceDetails(Guid Id, string Type, string Source, string Description, string Reference, string IntegrityHash, DateTimeOffset CollectedAtUtc);
