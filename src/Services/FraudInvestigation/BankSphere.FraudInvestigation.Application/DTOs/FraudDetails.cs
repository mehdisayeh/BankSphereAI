using BankSphere.FraudInvestigation.Domain;

namespace BankSphere.FraudInvestigation.Application;

public sealed record FraudDetails(Guid Id, string CaseNumber, Guid CustomerId, Guid AccountId, Guid TransactionId,
    string CaseType, string Category, InvestigationStatus Status, bool IsActive, FraudSeverity Severity, int Priority,
    decimal RiskScore, string RiskLevel, string Description, decimal SuspectedAmount, decimal Exposure, string Currency,
    string ReportedBy, DateTimeOffset ReviewDueAtUtc, DateTimeOffset CreatedAtUtc, long Version,
    IReadOnlyList<ReviewDetails> HumanReviews, IReadOnlyList<EvidenceDetails> Evidence)
{
    public static FraudDetails From(FraudCase fraudCase) => new(fraudCase.Id, fraudCase.CaseNumber, fraudCase.CustomerId, fraudCase.AccountId,
        fraudCase.TransactionId, fraudCase.CaseType, fraudCase.Category, fraudCase.Status, fraudCase.IsActive, fraudCase.Severity, fraudCase.Priority,
        fraudCase.RiskScore, fraudCase.RiskLevel, fraudCase.Description, fraudCase.SuspectedFraudAmount.Amount, fraudCase.EstimatedFinancialExposure.Amount,
        fraudCase.SuspectedFraudAmount.Currency.Code, fraudCase.ReportedBy, fraudCase.ReviewDueAtUtc, fraudCase.CreatedAtUtc, fraudCase.Version,
        fraudCase.HumanReviews.OrderBy(item => item.RequestedAtUtc).Select(item => new ReviewDetails(item.Id, item.Reviewer, item.Reason, item.RequestedAtUtc, item.Decision, item.DecidedAtUtc)).ToArray(),
        fraudCase.Evidence.Select(item => new EvidenceDetails(item.Id, item.Type, item.Source, item.Description, item.ContentReference, item.IntegrityHash, item.CollectedAtUtc)).ToArray());
}
