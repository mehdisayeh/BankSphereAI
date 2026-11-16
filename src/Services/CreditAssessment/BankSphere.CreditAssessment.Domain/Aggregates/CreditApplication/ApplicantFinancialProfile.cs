using BankSphere.SharedKernel;

namespace BankSphere.CreditAssessment.Domain;

public sealed record ApplicantFinancialProfile(string EmploymentType, Money DeclaredMonthlyIncome, Money DeclaredMonthlyExpenses,
    Money ExistingMonthlyDebtPayments, Money HousingExpenses)
{
    private ApplicantFinancialProfile() : this("", null!, null!, null!, null!) { }
}
