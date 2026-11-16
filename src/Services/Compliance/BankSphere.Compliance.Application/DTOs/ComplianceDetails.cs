using BankSphere.Compliance.Domain;

namespace BankSphere.Compliance.Application;

public sealed record ComplianceDetails(Guid Id, string CaseNumber, Guid CustomerId, ComplianceCaseType Type, ComplianceStatus Status,
    RiskClassification Risk, string Jurisdiction, string Reason, DateTimeOffset DueAtUtc, string? Decision, string? Rationale, long Version)
{
    public static ComplianceDetails From(ComplianceCase value) => new(value.Id, value.CaseNumber, value.CustomerId, value.Type, value.Status,
        value.RiskClassification, value.RegulatoryJurisdiction, value.ReviewReason, value.DueAtUtc, value.Decision, value.DecisionRationale, value.Version);
}
