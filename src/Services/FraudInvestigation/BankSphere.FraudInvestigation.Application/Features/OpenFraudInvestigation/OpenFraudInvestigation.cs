using BankSphere.FraudInvestigation.Domain;

namespace BankSphere.FraudInvestigation.Application;

public sealed record OpenFraudInvestigation(Guid TransactionId, string CaseType, string Category, FraudSeverity Severity,
    int Priority, string DetectionSource, DateTimeOffset DetectedAtUtc, decimal RiskScore, string RiskLevel,
    string Description, decimal SuspectedFraudAmount, decimal EstimatedFinancialExposure, DateTimeOffset ReviewDueAtUtc);
