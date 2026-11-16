
namespace BankSphere.CreditAssessment.Domain;

public interface ICreditAssessmentPolicy
{
    string Version { get; }
    DebtToIncomeAssessment DebtToIncome(ApplicantFinancialProfile profile);
    AffordabilityAssessment Affordability(ApplicantFinancialProfile profile);
}
