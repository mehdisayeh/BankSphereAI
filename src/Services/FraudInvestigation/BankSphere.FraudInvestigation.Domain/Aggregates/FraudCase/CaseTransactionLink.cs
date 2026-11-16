
namespace BankSphere.FraudInvestigation.Domain;

public sealed record CaseTransactionLink(Guid Id, Guid TransactionId, string AssociationReason);
