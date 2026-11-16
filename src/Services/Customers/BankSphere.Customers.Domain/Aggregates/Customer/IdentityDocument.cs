
namespace BankSphere.Customers.Domain;

public sealed record IdentityDocument(Guid Id, string DocumentType, string IssuingCountry, DateOnly IssueDate,
    DateOnly ExpiryDate, string ProtectedReference, bool Verified);
