
namespace BankSphere.CreditAssessment.Domain;

public sealed record CollateralReference(Guid Id, string Type, string ProtectedReference, decimal DeclaredValue, string CurrencyCode);
