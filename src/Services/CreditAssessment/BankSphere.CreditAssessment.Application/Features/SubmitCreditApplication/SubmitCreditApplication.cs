
namespace BankSphere.CreditAssessment.Application;

public sealed record SubmitCreditApplication(Guid CustomerId, string LoanProductId, string LoanPurpose, decimal RequestedAmount,
    string Currency, int TermMonths, string EmploymentType, decimal MonthlyIncome, decimal MonthlyExpenses,
    decimal MonthlyDebtPayments, decimal HousingExpenses, string SubmissionChannel, IReadOnlyList<IncomeInput> IncomeSources,
    IReadOnlyList<DebtInput> ExistingDebts, IReadOnlyList<ObligationInput> Obligations, IReadOnlyList<DocumentInput> Documents);
