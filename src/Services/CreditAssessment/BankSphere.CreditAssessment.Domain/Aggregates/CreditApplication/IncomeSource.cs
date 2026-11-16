
namespace BankSphere.CreditAssessment.Domain;

public sealed record IncomeSource(Guid Id, string Type, decimal MonthlyAmount, string CurrencyCode, string EvidenceReference);
