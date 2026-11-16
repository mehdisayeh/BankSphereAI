
namespace BankSphere.CreditAssessment.Domain;

public sealed record FinancialObligation(Guid Id, string Type, decimal MonthlyAmount, string CurrencyCode);
