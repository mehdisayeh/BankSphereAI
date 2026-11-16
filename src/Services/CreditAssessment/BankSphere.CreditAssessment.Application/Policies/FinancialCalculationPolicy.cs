using BankSphere.CreditAssessment.Domain;
using BankSphere.SharedKernel;

namespace BankSphere.CreditAssessment.Application;

// A configurable calculation policy, not a lending approval threshold or credit score.
public sealed class FinancialCalculationPolicy(string version, bool includeHousing) : ICreditAssessmentPolicy
{
    public string Version { get; } = Guard.Text(version, "Policy version");
    public DebtToIncomeAssessment DebtToIncome(ApplicantFinancialProfile profile)
    {
        profile.DeclaredMonthlyIncome.Add(profile.ExistingMonthlyDebtPayments);
        Guard.Against(profile.DeclaredMonthlyIncome.Amount == 0, "credit.income_required", "Debt-to-income ratio requires nonzero declared income.");
        return new(profile.ExistingMonthlyDebtPayments.Amount / profile.DeclaredMonthlyIncome.Amount, Version);
    }
    public AffordabilityAssessment Affordability(ApplicantFinancialProfile profile)
    {
        profile.DeclaredMonthlyIncome.Add(profile.DeclaredMonthlyExpenses).Add(profile.ExistingMonthlyDebtPayments).Add(profile.HousingExpenses);
        return new(profile.DeclaredMonthlyIncome.Amount - profile.DeclaredMonthlyExpenses.Amount - profile.ExistingMonthlyDebtPayments.Amount
            - (includeHousing ? profile.HousingExpenses.Amount : 0), profile.DeclaredMonthlyIncome.Currency.Code, Version);
    }
}
