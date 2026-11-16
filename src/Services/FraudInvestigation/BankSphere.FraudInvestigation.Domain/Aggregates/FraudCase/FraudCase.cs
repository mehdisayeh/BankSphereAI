using BankSphere.SharedKernel;

namespace BankSphere.FraudInvestigation.Domain;

public sealed class FraudCase : BankingAggregate
{
    private readonly List<FraudIndicator> indicators = [];
    private readonly List<InvestigationEvidence> evidence = [];
    private readonly List<InvestigationNote> notes = [];
    private readonly List<FraudCaseAssignment> assignments = [];
    private readonly List<FraudCaseStatusHistory> statusHistory = [];
    private readonly List<HumanReviewRequest> humanReviews = [];
    private readonly List<FraudDecision> decisions = [];
    private readonly List<CaseTransactionLink> transactionLinks = [];
    private FraudCase() { }
    public string CaseNumber { get; private set; } = "";
    public Guid CustomerId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid TransactionId { get; private set; }
    public string CaseType { get; private set; } = "";
    public string Category { get; private set; } = "";
    public InvestigationStatus Status { get; private set; }
    public bool IsActive { get; private set; } = true;
    public FraudSeverity Severity { get; private set; }
    public int Priority { get; private set; }
    public string? AssignedInvestigatorId { get; private set; }
    public string? AssignedTeamId { get; private set; }
    public string DetectionSource { get; private set; } = "";
    public DateTimeOffset DetectionTimestampUtc { get; private set; }
    public decimal RiskScore { get; private set; }
    public string RiskLevel { get; private set; } = "";
    public string Description { get; private set; } = "";
    public Money SuspectedFraudAmount { get; private set; } = null!;
    public Money EstimatedFinancialExposure { get; private set; } = null!;
    public string ReportedBy { get; private set; } = "";
    public DateTimeOffset InvestigationStartedAtUtc { get; private set; }
    public DateTimeOffset? InvestigationClosedAtUtc { get; private set; }
    public string? ResolutionCode { get; private set; }
    public string? ResolutionSummary { get; private set; }
    public DateTimeOffset ReviewDueAtUtc { get; private set; }
    public IReadOnlyCollection<FraudIndicator> Indicators => indicators.AsReadOnly();
    public IReadOnlyCollection<InvestigationEvidence> Evidence => evidence.AsReadOnly();
    public IReadOnlyCollection<InvestigationNote> Notes => notes.AsReadOnly();
    public IReadOnlyCollection<FraudCaseAssignment> Assignments => assignments.AsReadOnly();
    public IReadOnlyCollection<FraudCaseStatusHistory> StatusHistory => statusHistory.AsReadOnly();
    public IReadOnlyCollection<HumanReviewRequest> HumanReviews => humanReviews.AsReadOnly();
    public IReadOnlyCollection<FraudDecision> Decisions => decisions.AsReadOnly();
    public IReadOnlyCollection<CaseTransactionLink> TransactionLinks => transactionLinks.AsReadOnly();

    public static FraudCase OpenInvestigation(Guid customer, Guid account, Guid transaction, string type, string category,
        FraudSeverity severity, int priority, string detectionSource, DateTimeOffset detectedAt, decimal riskScore,
        string riskLevel, string description, Money suspected, Money exposure, string reporter, DateTimeOffset reviewDue, DateTimeOffset now)
    {
        suspected.Add(exposure);
        Guard.Against(priority < 1 || priority > 5 || riskScore < 0 || riskScore > 100, "fraud.scoring", "Priority must be 1..5 and risk score 0..100.");
        Guard.Against(detectedAt > now || reviewDue <= now, "fraud.dates", "Detection cannot be future; review due must be future.");
        var fraudCase = new FraudCase { CustomerId = Guard.Id(customer, "Customer"), AccountId = Guard.Id(account, "Account"), TransactionId = Guard.Id(transaction, "Transaction"),
            CaseType = Guard.Text(type, "Case type"), Category = Guard.Text(category, "Category", 80).ToUpperInvariant(), Severity = Guard.Defined(severity), Priority = priority,
            DetectionSource = Guard.Text(detectionSource, "Source"), DetectionTimestampUtc = Guard.Utc(detectedAt), RiskScore = riskScore,
            RiskLevel = Guard.Text(riskLevel, "Risk level"), Description = Guard.Text(description, "Description", 2000), SuspectedFraudAmount = suspected,
            EstimatedFinancialExposure = exposure, ReportedBy = Guard.Text(reporter, "Reporter"), ReviewDueAtUtc = Guard.Utc(reviewDue), InvestigationStartedAtUtc = Guard.Utc(now) };
        fraudCase.CaseNumber = $"FRD-{fraudCase.Id:N}"; fraudCase.Initialize(now);
        fraudCase.transactionLinks.Add(new(Guid.NewGuid(), transaction, "Primary investigated transaction"));
        fraudCase.RaiseDomainEvent(new InvestigationOpened(fraudCase.Id, Guid.NewGuid(), now)); return fraudCase;
    }

    private void Editable() => Guard.Against(!IsActive, "fraud.inactive", "Resolved or closed investigations cannot change.", FailureKind.Conflict);

    private void Transition(InvestigationStatus next, string actor, string reason, DateTimeOffset now, params InvestigationStatus[] allowed)
    {
        Guard.Against(!allowed.Contains(Status), "fraud.transition", $"Cannot change {Status} to {next}.", FailureKind.Conflict);
        statusHistory.Add(new(Guid.NewGuid(), Status, next, Guard.Text(actor, "Actor"), Guard.Text(reason, "Reason", 2000), Guard.Utc(now)));
        Status = next; Changed(now);
    }

    public void AssignInvestigator(string investigator, string team, DateTimeOffset now)
    {
        Editable();
        AssignedInvestigatorId = Guard.Text(investigator, "Investigator"); AssignedTeamId = Guard.Text(team, "Team");
        assignments.Add(new(Guid.NewGuid(), investigator, team, Guard.Utc(now))); Changed(now);
    }

    public void AddFraudIndicator(string type, FraudSeverity severity, string reference, DateTimeOffset now)
    {
        Editable();
        indicators.Add(new(Guid.NewGuid(), Guard.Text(type, "Indicator"), Guard.Defined(severity), Guard.Text(reference, "Evidence reference"), Guard.Utc(now))); Changed(now);
    }

    public void AttachEvidence(string type, string source, string description, string reference, string hash, DateTimeOffset now)
    {
        Editable();
        Guard.Against(hash.Length != 64 || !hash.All(Uri.IsHexDigit), "fraud.hash", "Evidence requires a SHA-256 hex digest.");
        evidence.Add(new(Guid.NewGuid(), Guard.Text(type, "Evidence type"), Guard.Text(source, "Source"), Guard.Text(description, "Description", 2000), Guard.Text(reference, "Immutable reference"), hash.ToUpperInvariant(), Guard.Utc(now))); Changed(now);
    }

    public void AddNote(string author, string content, string visibility, DateTimeOffset now)
    {
        Editable();
        notes.Add(new(Guid.NewGuid(), Guard.Text(author, "Author"), Guard.Text(content, "Note", 2000), Guard.Text(visibility, "Visibility"), Guard.Utc(now))); Changed(now);
    }

    public void RequestHumanReview(string reviewer, string reason, string actor, DateTimeOffset now)
    {
        reviewer = Guard.Text(reviewer, "Reviewer");
        Transition(InvestigationStatus.AwaitingHumanReview, actor, reason, now, InvestigationStatus.Investigating, InvestigationStatus.Escalated);
        humanReviews.Add(new(Guid.NewGuid(), reviewer, reason, now, null, null));
        RaiseDomainEvent(new HumanReviewRequested(Id, reviewer, Guid.NewGuid(), now));
    }

    public void RecordHumanDecision(FraudDecisionClassification classification, string rationale, string reviewer, string evidenceReference, DateTimeOffset now)
    {
        Guard.Defined(classification); rationale = Guard.Text(rationale, "Rationale", 2000); evidenceReference = Guard.Text(evidenceReference, "Evidence reference");
        Guard.Against(Status != InvestigationStatus.AwaitingHumanReview || humanReviews.Count == 0 || humanReviews[^1].Reviewer != reviewer, "fraud.reviewer", "Only the assigned reviewer can record a decision on a pending review.", FailureKind.Forbidden);
        Guard.Against(!evidence.Any(item => item.ContentReference == evidenceReference), "fraud.evidence", "Decision must reference attached evidence.");
        humanReviews[^1] = humanReviews[^1] with { Decision = classification.ToString(), DecidedAtUtc = Guard.Utc(now) };
        decisions.Add(new(Guid.NewGuid(), classification, rationale, reviewer, evidenceReference, now));
        Transition(InvestigationStatus.Investigating, reviewer, "Human decision recorded", now, InvestigationStatus.AwaitingHumanReview);
    }

    public void EscalateInvestigation(string actor, string reason, DateTimeOffset now) => Transition(InvestigationStatus.Escalated, actor, reason, now, InvestigationStatus.Investigating);

    public void ResolveInvestigation(string code, string summary, string actor, DateTimeOffset now)
    {
        code = Guard.Text(code, "Resolution code"); summary = Guard.Text(summary, "Resolution summary", 2000);
        Guard.Against(decisions.Count == 0, "fraud.decision_required", "A human decision is required before resolution.", FailureKind.Conflict);
        Transition(InvestigationStatus.Resolved, actor, summary, now, InvestigationStatus.Investigating, InvestigationStatus.Escalated);
        ResolutionCode = code; ResolutionSummary = summary; IsActive = false;
    }

    public void CloseInvestigation(string actor, string reason, DateTimeOffset now)
    {
        Transition(InvestigationStatus.Closed, actor, reason, now, InvestigationStatus.Resolved); InvestigationClosedAtUtc = now;
    }

    public void ReopenInvestigation(string actor, string reason, DateTimeOffset now)
    {
        Transition(InvestigationStatus.Investigating, actor, reason, now, InvestigationStatus.Resolved, InvestigationStatus.Closed);
        IsActive = true; InvestigationClosedAtUtc = null;
    }
}
