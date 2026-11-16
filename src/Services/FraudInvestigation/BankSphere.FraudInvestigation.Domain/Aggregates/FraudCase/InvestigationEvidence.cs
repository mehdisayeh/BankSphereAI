
namespace BankSphere.FraudInvestigation.Domain;

public sealed record InvestigationEvidence(Guid Id, string Type, string Source, string Description, string ContentReference, string IntegrityHash, DateTimeOffset CollectedAtUtc);
