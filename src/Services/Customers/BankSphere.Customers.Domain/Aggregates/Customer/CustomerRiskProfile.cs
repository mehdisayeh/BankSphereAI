using BankSphere.SharedKernel;

namespace BankSphere.Customers.Domain;

public sealed class CustomerRiskProfile
{
    private readonly List<RiskFactor> factors = [];
    private CustomerRiskProfile() { }
    public Guid Id { get; private set; } = Guid.NewGuid();
    public CustomerRiskLevel Level { get; private set; }
    public string Methodology { get; private set; } = "";
    public string Rationale { get; private set; } = "";
    public DateTimeOffset AssessedAtUtc { get; private set; }
    public DateTimeOffset ReassessAtUtc { get; private set; }
    public IReadOnlyCollection<RiskFactor> Factors => factors.AsReadOnly();
    public static CustomerRiskProfile Assess(CustomerRiskLevel level, string methodology, string rationale,
        IEnumerable<RiskFactor> factors, DateTimeOffset now, DateTimeOffset reassessAtUtc)
    {
        Guard.Against(reassessAtUtc <= now, "customer.risk_review", "Reassessment must be in the future.");
        var profile = new CustomerRiskProfile { Level = Guard.Defined(level), Methodology = Guard.Text(methodology, "Methodology"),
            Rationale = Guard.Text(rationale, "Rationale", 2000), AssessedAtUtc = Guard.Utc(now), ReassessAtUtc = Guard.Utc(reassessAtUtc) };
        foreach (var factor in factors) profile.factors.Add(new(Guard.Id(factor.Id, "Factor"), Guard.Text(factor.Code, "Factor code"), Guard.Text(factor.Rationale, "Rationale", 2000)));
        Guard.Against(profile.factors.Count == 0, "customer.risk_factors", "Risk assessment requires factors.");
        return profile;
    }
}
