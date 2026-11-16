using BankSphere.SharedKernel;

namespace BankSphere.Compliance.Domain;

public sealed class ComplianceCase : BankingAggregate
{
    private readonly List<KycReview> kycReviews = [];
    private readonly List<CustomerDueDiligence> dueDiligence = [];
    private readonly List<ScreeningResult> screenings = [];
    private readonly List<ComplianceDocumentReference> documents = [];
    private readonly List<ComplianceReviewHistory> reviewHistory = [];
    private readonly List<RiskAssessmentFactor> riskFactors = [];
    private ComplianceCase() { }
    public string CaseNumber { get; private set; } = "";
    public Guid CustomerId { get; private set; }
    public ComplianceCaseType Type { get; private set; }
    public ComplianceStatus Status { get; private set; }
    public RiskClassification RiskClassification { get; private set; }
    public string RegulatoryJurisdiction { get; private set; } = "";
    public string ReviewReason { get; private set; } = "";
    public string ScreeningSource { get; private set; } = "";
    public string? ComplianceOfficerId { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public DateTimeOffset OpenedAtUtc { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public string? Decision { get; private set; }
    public string? DecisionRationale { get; private set; }
    public IReadOnlyCollection<KycReview> KycReviews => kycReviews.AsReadOnly();
    public IReadOnlyCollection<CustomerDueDiligence> DueDiligence => dueDiligence.AsReadOnly();
    public IReadOnlyCollection<ScreeningResult> Screenings => screenings.AsReadOnly();
    public IReadOnlyCollection<ComplianceDocumentReference> Documents => documents.AsReadOnly();
    public IReadOnlyCollection<ComplianceReviewHistory> ReviewHistory => reviewHistory.AsReadOnly();
    public IReadOnlyCollection<RiskAssessmentFactor> RiskFactors => riskFactors.AsReadOnly();

    public static ComplianceCase Open(Guid customer, ComplianceCaseType type, string jurisdiction, string reason, string source, DateTimeOffset due, DateTimeOffset now)
    {
        Guard.Against(due <= now, "compliance.due", "Review due date must be in the future.");
        var result = new ComplianceCase { CustomerId = Guard.Id(customer, "Customer"), Type = Guard.Defined(type), RegulatoryJurisdiction = Guard.Country(jurisdiction),
            ReviewReason = Guard.Text(reason, "Reason", 2000), ScreeningSource = Guard.Text(source, "Screening source"), DueAtUtc = Guard.Utc(due), OpenedAtUtc = Guard.Utc(now) };
        result.CaseNumber = $"CMP-{result.Id:N}"; result.Initialize(now); return result;
    }
    private void Editable() => Guard.Against(Status is ComplianceStatus.Approved or ComplianceStatus.Rejected or ComplianceStatus.Closed, "compliance.terminal", "Completed compliance reviews cannot change.", FailureKind.Conflict);
    private void Transition(ComplianceStatus status, string officer, string reason, DateTimeOffset now)
    {
        reviewHistory.Add(new(Guid.NewGuid(), Status, status, Guard.Text(officer, "Officer"), Guard.Text(reason, "Reason", 2000), Guard.Utc(now)));
        Status = status; ComplianceOfficerId = officer; Changed(now);
    }
    public void BeginReview(string officer, DateTimeOffset now)
    {
        Guard.Against(Status is not (ComplianceStatus.Open or ComplianceStatus.AwaitingEvidence), "compliance.transition", "Only open or evidence-pending cases may begin review.", FailureKind.Conflict);
        Transition(ComplianceStatus.UnderReview, officer, "Review started", now);
    }
    public void RequestEvidence(string officer, string reason, DateTimeOffset now)
    {
        Guard.Against(Status != ComplianceStatus.UnderReview, "compliance.transition", "Evidence requests require an active review.", FailureKind.Conflict);
        Transition(ComplianceStatus.AwaitingEvidence, officer, reason, now);
    }
    public void RecordKycReview(string officer, string findings, string reference, DateTimeOffset nextReview, DateTimeOffset now)
    {
        Editable(); Guard.Against(nextReview <= now, "compliance.kyc_due", "Next KYC review must be in the future.");
        kycReviews.Add(new(Guid.NewGuid(), Guard.Utc(now), Guard.Utc(nextReview), Guard.Text(officer, "Officer"), Guard.Text(findings, "Findings", 2000), Guard.Text(reference, "Evidence"))); Changed(now);
    }
    public void RecordDueDiligence(bool enhanced, string reason, string sourceOfFunds, string officer, string evidence, DateTimeOffset now)
    {
        Editable(); dueDiligence.Add(new(Guid.NewGuid(), enhanced, Guard.Text(reason, "Reason"), Guard.Text(sourceOfFunds, "Funds assessment", 2000), Guard.Text(officer, "Officer"), Guard.Text(evidence, "Evidence"), Guard.Utc(now))); Changed(now);
    }
    public void RecordScreening(string type, string source, string reference, bool match, string disposition, string officer, DateTimeOffset now)
    {
        Editable(); Guard.Against(type is not ("sanctions" or "pep" or "adverse-media"), "compliance.screening_type", "Unsupported screening type.");
        screenings.Add(new(Guid.NewGuid(), type, Guard.Text(source, "Source"), Guard.Text(reference, "Protected reference"), match, Guard.Text(disposition, "Disposition", 2000), Guard.Text(officer, "Officer"), Guard.Utc(now))); Changed(now);
    }
    public void AttachDocument(string type, string reference, DateTimeOffset now)
    {
        Editable(); documents.Add(new(Guid.NewGuid(), Guard.Text(type, "Type"), Guard.Text(reference, "Protected reference"), Guard.Utc(now))); Changed(now);
    }
    public void AssessRisk(RiskClassification risk, string code, string rationale, string assessor, DateTimeOffset now)
    {
        Editable(); riskFactors.Add(new(Guid.NewGuid(), Guard.Defined(risk), Guard.Text(code, "Factor"), Guard.Text(rationale, "Rationale", 2000), Guard.Text(assessor, "Assessor"), Guard.Utc(now)));
        RiskClassification = risk; Changed(now);
    }
    public void RecordDecision(bool approve, string officer, string rationale, DateTimeOffset now)
    {
        Guard.Against(Status != ComplianceStatus.UnderReview || kycReviews.Count == 0 || documents.Count == 0 || riskFactors.Count == 0,
            "compliance.decision_prerequisites", "Active review, KYC, evidence documents and risk assessment are required.", FailureKind.Conflict);
        Guard.Against(approve && (dueDiligence.Count == 0 || (RiskClassification == RiskClassification.High && !dueDiligence.Any(item => item.Enhanced))),
            "compliance.diligence", "Approval requires diligence and enhanced diligence for high risk.", FailureKind.Conflict);
        rationale = Guard.Text(rationale, "Decision rationale", 2000);
        Transition(approve ? ComplianceStatus.Approved : ComplianceStatus.Rejected, officer, rationale, now);
        Decision = approve ? "Approved" : "Rejected"; DecisionRationale = rationale; ReviewedAtUtc = now;
    }
    public void Close(string officer, DateTimeOffset now)
    {
        Guard.Against(Status is not (ComplianceStatus.Approved or ComplianceStatus.Rejected), "compliance.transition", "A decision is required before closing.", FailureKind.Conflict);
        Transition(ComplianceStatus.Closed, officer, "Review closed", now); ClosedAtUtc = now;
    }
}
