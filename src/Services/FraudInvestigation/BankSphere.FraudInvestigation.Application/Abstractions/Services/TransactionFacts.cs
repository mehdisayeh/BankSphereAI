
namespace BankSphere.FraudInvestigation.Application;

public sealed record TransactionFacts(Guid Id, Guid AccountId, Guid CustomerId, decimal Amount, string Currency, bool IsSynthetic);
