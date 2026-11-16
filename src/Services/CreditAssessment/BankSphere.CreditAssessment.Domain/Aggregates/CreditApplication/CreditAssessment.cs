using BankSphere.SharedKernel;

namespace BankSphere.CreditAssessment.Domain;

public sealed class CreditAssessment
{
    private readonly List<CreditRiskFactor> riskFactors = [];
    private CreditAssessment() { }
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Assessor { get; private set; } = "";
    public string PolicyVersion { get; private set; } = "";
    public decimal DebtToIncomeRatio { get; private set; }
    public decimal DisposableMonthlyIncome { get; private set; }
    public string CurrencyCode { get; private set; } = "";
    public DateTimeOffset AssessedAtUtc { get; private set; }
    public IReadOnlyCollection<CreditRiskFactor> RiskFactors => riskFactors.AsReadOnly();

    public static CreditAssessment Evaluate(ApplicantFinancialProfile financial, ICreditAssessmentPolicy policy,
        string assessor, IEnumerable<CreditRiskFactor> factors, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(financial); ArgumentNullException.ThrowIfNull(policy);
        var assessment = new CreditAssessment { Assessor = Guard.Text(assessor, "Assessor"), PolicyVersion = Guard.Text(policy.Version, "Policy version"),
            DebtToIncomeRatio = policy.DebtToIncome(financial).Ratio, DisposableMonthlyIncome = policy.Affordability(financial).DisposableMonthlyIncome,
            CurrencyCode = financial.DeclaredMonthlyIncome.Currency.Code, AssessedAtUtc = Guard.Utc(now) };
        foreach (var factor in factors) assessment.riskFactors.Add(new(Guard.Id(factor.Id, "Factor"), Guard.Text(factor.Code, "Factor code"), Guard.Text(factor.Rationale, "Rationale", 2000), Guard.Text(factor.EvidenceReference, "Evidence")));
        Guard.Against(assessment.riskFactors.Count == 0, "credit.factors", "Assessment requires evidenced risk factors."); return assessment;
    }
}
