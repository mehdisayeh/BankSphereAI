
namespace BankSphere.CreditAssessment.Domain;

public sealed record ExistingDebt(Guid Id, string LenderReference, decimal OutstandingAmount, decimal MonthlyPayment, string CurrencyCode);
