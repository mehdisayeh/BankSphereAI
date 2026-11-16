
namespace BankSphere.FraudInvestigation.Domain;

public sealed record FraudCaseStatusHistory(Guid Id, InvestigationStatus PreviousStatus, InvestigationStatus NewStatus, string Actor, string Reason, DateTimeOffset AtUtc);
