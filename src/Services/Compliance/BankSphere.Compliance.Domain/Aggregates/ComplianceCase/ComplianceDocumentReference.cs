
namespace BankSphere.Compliance.Domain;

public sealed record ComplianceDocumentReference(Guid Id, string Type, string ProtectedReference, DateTimeOffset AddedAtUtc);
