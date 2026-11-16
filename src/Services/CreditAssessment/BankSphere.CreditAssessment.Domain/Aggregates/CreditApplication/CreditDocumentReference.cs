
namespace BankSphere.CreditAssessment.Domain;

public sealed record CreditDocumentReference(Guid Id, string Type, string ProtectedReference, DateTimeOffset AddedAtUtc);
