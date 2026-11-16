
namespace BankSphere.FraudInvestigation.Domain;

public sealed record FraudCaseAssignment(Guid Id, string Investigator, string Team, DateTimeOffset AssignedAtUtc);
