
namespace BankSphere.CreditAssessment.Application;

public sealed record DebtInput(string LenderReference, decimal OutstandingAmount, decimal MonthlyPayment);
