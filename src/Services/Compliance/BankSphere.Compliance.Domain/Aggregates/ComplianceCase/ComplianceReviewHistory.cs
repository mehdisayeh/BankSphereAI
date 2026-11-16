
namespace BankSphere.Compliance.Domain;

public sealed record ComplianceReviewHistory(Guid Id, ComplianceStatus PreviousStatus, ComplianceStatus NewStatus, string Actor, string Reason, DateTimeOffset AtUtc);
