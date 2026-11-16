
namespace BankSphere.FraudInvestigation.Domain;

public sealed record InvestigationNote(Guid Id, string Author, string Content, string Visibility, DateTimeOffset AtUtc);
