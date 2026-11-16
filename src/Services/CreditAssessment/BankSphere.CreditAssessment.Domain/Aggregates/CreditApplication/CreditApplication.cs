using BankSphere.SharedKernel;

namespace BankSphere.CreditAssessment.Domain;

public sealed class CreditApplication : BankingAggregate
{
    private readonly List<IncomeSource> incomeSources = [];
    private readonly List<ExistingDebt> existingDebts = [];
    private readonly List<FinancialObligation> obligations = [];
    private readonly List<CollateralReference> collateral = [];
    private readonly List<CreditDocumentReference> documents = [];
    private readonly List<CreditAssessment> assessments = [];
    private readonly List<CreditRecommendation> recommendations = [];
    private readonly List<CreditReviewRequest> reviewRequests = [];
    private readonly List<CreditDecisionHistory> decisionHistory = [];
    private CreditApplication() { }
    public string ApplicationNumber { get; private set; } = "";
    public Guid CustomerId { get; private set; }
    public string LoanProductId { get; private set; } = "";
    public string LoanPurpose { get; private set; } = "";
    public Money RequestedAmount { get; private set; } = null!;
    public int RequestedTermMonths { get; private set; }
    public CreditApplicationStatus Status { get; private set; }
    public ApplicantFinancialProfile Financial { get; private set; } = null!;
    public string SubmissionChannel { get; private set; } = "";
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public string? AssignedCreditOfficerId { get; private set; }
    public IReadOnlyCollection<IncomeSource> IncomeSources => incomeSources.AsReadOnly();
    public IReadOnlyCollection<ExistingDebt> ExistingDebts => existingDebts.AsReadOnly();
    public IReadOnlyCollection<FinancialObligation> Obligations => obligations.AsReadOnly();
    public IReadOnlyCollection<CollateralReference> Collateral => collateral.AsReadOnly();
    public IReadOnlyCollection<CreditDocumentReference> Documents => documents.AsReadOnly();
    public IReadOnlyCollection<CreditAssessment> Assessments => assessments.AsReadOnly();
    public IReadOnlyCollection<CreditRecommendation> Recommendations => recommendations.AsReadOnly();
    public IReadOnlyCollection<CreditReviewRequest> ReviewRequests => reviewRequests.AsReadOnly();
    public IReadOnlyCollection<CreditDecisionHistory> DecisionHistory => decisionHistory.AsReadOnly();

    public static CreditApplication CreateDraftApplication(Guid customer, string product, string purpose, Money amount,
        int months, string channel, ApplicantFinancialProfile financial, DateTimeOffset now)
    {
        Guard.Against(amount.Amount <= 0 || months < 1 || months > 600, "credit.terms", "Amount must be positive; term must be 1..600 months (supported product envelope).");
        var application = new CreditApplication { CustomerId = Guard.Id(customer, "Customer"), LoanProductId = Guard.Text(product, "Product"), LoanPurpose = Guard.Text(purpose, "Purpose"),
            RequestedAmount = amount, RequestedTermMonths = months, SubmissionChannel = Guard.Text(channel, "Channel") };
        application.ApplicationNumber = $"CRD-{application.Id:N}"; application.Initialize(now); application.UpdateFinancialInformation(financial, now); return application;
    }
    private void DraftEditable() => Guard.Against(Status is not (CreditApplicationStatus.Draft or CreditApplicationStatus.AwaitingDocuments), "credit.immutable_financials", "Financial data may change only in draft or when documents are requested.", FailureKind.Conflict);
    public void UpdateFinancialInformation(ApplicantFinancialProfile profile, DateTimeOffset now)
    {
        DraftEditable(); ArgumentNullException.ThrowIfNull(profile);
        Guard.Text(profile.EmploymentType, "Employment type");
        RequestedAmount.Add(profile.DeclaredMonthlyIncome).Add(profile.DeclaredMonthlyExpenses).Add(profile.ExistingMonthlyDebtPayments).Add(profile.HousingExpenses);
        Financial = profile; Changed(now);
    }
    public void AddIncomeSource(string type, Money monthly, string reference, DateTimeOffset now)
    {
        DraftEditable(); RequestedAmount.Add(monthly); incomeSources.Add(new(Guid.NewGuid(), Guard.Text(type, "Income type"), monthly.Amount, monthly.Currency.Code, Guard.Text(reference, "Income evidence"))); Changed(now);
    }
    public void AddExistingDebt(string lender, Money outstanding, Money payment, DateTimeOffset now)
    {
        DraftEditable(); RequestedAmount.Add(outstanding).Add(payment); existingDebts.Add(new(Guid.NewGuid(), Guard.Text(lender, "Lender reference"), outstanding.Amount, payment.Amount, payment.Currency.Code)); Changed(now);
    }
    public void AddObligation(string type, Money monthly, DateTimeOffset now)
    {
        DraftEditable(); RequestedAmount.Add(monthly); obligations.Add(new(Guid.NewGuid(), Guard.Text(type, "Obligation"), monthly.Amount, monthly.Currency.Code)); Changed(now);
    }
    public void AddCollateral(string type, string reference, Money value, DateTimeOffset now)
    {
        DraftEditable(); RequestedAmount.Add(value); collateral.Add(new(Guid.NewGuid(), Guard.Text(type, "Collateral"), Guard.Text(reference, "Protected reference"), value.Amount, value.Currency.Code)); Changed(now);
    }
    public void AttachDocument(string type, string reference, DateTimeOffset now)
    {
        DraftEditable(); documents.Add(new(Guid.NewGuid(), Guard.Text(type, "Document type"), Guard.Text(reference, "Protected reference"), Guard.Utc(now))); Changed(now);
    }
    public void SubmitApplication(DateTimeOffset now)
    {
        Guard.Against(Status is not (CreditApplicationStatus.Draft or CreditApplicationStatus.AwaitingDocuments) || documents.Count == 0 || incomeSources.Count == 0,
            "credit.submission", "Submission requires draft/document-pending status, income evidence and documents.", FailureKind.Conflict);
        Status = CreditApplicationStatus.Submitted; SubmittedAtUtc = Guard.Utc(now); Changed(now);
        RaiseDomainEvent(new CreditApplicationSubmitted(Id, Guid.NewGuid(), now));
    }
    public void BeginAssessment(string officer, DateTimeOffset now)
    {
        Guard.Against(Status != CreditApplicationStatus.Submitted, "credit.transition", "Only submitted applications can enter assessment.", FailureKind.Conflict);
        AssignedCreditOfficerId = Guard.Text(officer, "Officer"); Status = CreditApplicationStatus.Assessing; Changed(now);
    }
    public void RequestAdditionalDocuments(string officer, string reason, DateTimeOffset now)
    {
        Guard.Against(Status is not (CreditApplicationStatus.Submitted or CreditApplicationStatus.Assessing), "credit.transition", "Documents can be requested after submission or during assessment.", FailureKind.Conflict);
        decisionHistory.Add(new(Guid.NewGuid(), CreditApplicationStatus.AwaitingDocuments, Guard.Text(reason, "Reason", 2000), "document-request", Guard.Text(officer, "Officer"), Guard.Utc(now)));
        Status = CreditApplicationStatus.AwaitingDocuments; Changed(now);
    }
    public void RecordAssessment(CreditAssessment assessment, DateTimeOffset now)
    {
        Guard.Against(Status != CreditApplicationStatus.Assessing, "credit.transition", "Assessment can be recorded only during assessment.", FailureKind.Conflict);
        ArgumentNullException.ThrowIfNull(assessment); Guard.Against(assessment.CurrencyCode != RequestedAmount.Currency.Code, "credit.currency", "Assessment currency mismatch."); assessments.Add(assessment); Changed(now);
    }
    public void RecommendDecision(CreditDecisionRecommendation recommendation, string rationale, string evidence, string actor, DateTimeOffset now)
    {
        Guard.Against(Status != CreditApplicationStatus.Assessing || assessments.Count == 0, "credit.assessment_required", "Recommendation requires a recorded assessment.", FailureKind.Conflict);
        recommendations.Add(new(Guid.NewGuid(), Guard.Defined(recommendation), Guard.Text(rationale, "Rationale", 2000), Guard.Text(evidence, "Evidence"), Guard.Text(actor, "Actor"), Guard.Utc(now))); Changed(now);
    }
    public void RequestHumanReview(string reviewer, string reason, DateTimeOffset now)
    {
        Guard.Against(Status != CreditApplicationStatus.Assessing || recommendations.Count == 0, "credit.recommendation_required", "Human review requires an assessment recommendation.", FailureKind.Conflict);
        reviewRequests.Add(new(Guid.NewGuid(), Guard.Text(reviewer, "Reviewer"), Guard.Text(reason, "Reason", 2000), Guard.Utc(now), null)); Status = CreditApplicationStatus.AwaitingHumanReview; Changed(now);
    }
    public void RecordHumanDecision(bool approved, string rationale, string evidence, string actor, DateTimeOffset now)
    {
        Guard.Against(Status != CreditApplicationStatus.AwaitingHumanReview || reviewRequests.Count == 0 || reviewRequests[^1].Reviewer != actor,
            "credit.reviewer", "Only the assigned reviewer can decide a pending human review.", FailureKind.Forbidden);
        rationale = Guard.Text(rationale, "Rationale", 2000); evidence = Guard.Text(evidence, "Evidence");
        Guard.Against(!documents.Any(document => document.ProtectedReference == evidence), "credit.evidence", "Decision must reference an attached document.");
        Status = approved ? CreditApplicationStatus.Approved : CreditApplicationStatus.Rejected;
        decisionHistory.Add(new(Guid.NewGuid(), Status, rationale, evidence, Guard.Text(actor, "Actor"), Guard.Utc(now)));
        reviewRequests[^1] = reviewRequests[^1] with { CompletedAtUtc = now }; ReviewedAtUtc = now; Changed(now);
    }
    public void ApproveApplication(string rationale, string evidence, string actor, DateTimeOffset now) => RecordHumanDecision(true, rationale, evidence, actor, now);
    public void RejectApplication(string rationale, string evidence, string actor, DateTimeOffset now) => RecordHumanDecision(false, rationale, evidence, actor, now);
    public void WithdrawApplication(string actor, string reason, DateTimeOffset now)
    {
        Guard.Against(Status is CreditApplicationStatus.Approved or CreditApplicationStatus.Rejected or CreditApplicationStatus.Withdrawn, "credit.transition", "Completed applications cannot be withdrawn.", FailureKind.Conflict);
        decisionHistory.Add(new(Guid.NewGuid(), CreditApplicationStatus.Withdrawn, Guard.Text(reason, "Reason", 2000), "withdrawal", Guard.Text(actor, "Actor"), Guard.Utc(now)));
        Status = CreditApplicationStatus.Withdrawn; Changed(now);
    }
}
