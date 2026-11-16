using BankSphere.Compliance.Domain;

namespace BankSphere.Compliance.Application;

public sealed record OpenComplianceCase(Guid CustomerId, ComplianceCaseType Type, string Jurisdiction, string Reason, string ScreeningSource, DateTimeOffset DueAtUtc);
