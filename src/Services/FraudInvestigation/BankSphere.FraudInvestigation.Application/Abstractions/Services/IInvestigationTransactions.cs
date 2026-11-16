
namespace BankSphere.FraudInvestigation.Application;

public interface IInvestigationTransactions
{
    Task<TransactionFacts?> FindAsync(Guid transaction, CancellationToken cancellationToken);
}
