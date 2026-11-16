
namespace BankSphere.CreditAssessment.Domain;

public sealed record AffordabilityAssessment(decimal DisposableMonthlyIncome, string CurrencyCode, string PolicyVersion);
