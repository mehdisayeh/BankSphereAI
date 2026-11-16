
namespace BankSphere.FraudInvestigation.Application;

public sealed record AttachFraudEvidence(string Type, string Source, string Description, string Reference, string Sha256, long ExpectedVersion);
