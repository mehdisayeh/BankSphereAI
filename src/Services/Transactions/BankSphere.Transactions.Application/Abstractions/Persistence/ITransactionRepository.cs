using BankSphere.SharedKernel;
using BankSphere.Transactions.Domain;

namespace BankSphere.Transactions.Application;

public interface ITransactionRepository
{
    Task<BankingTransaction?> FindByExternalReferenceAsync(string source, string reference, CancellationToken cancellationToken);
    Task AddAsync(BankingTransaction transaction, CancellationToken cancellationToken);
    Task<TransactionDetails?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<Page<TransactionDetails>> ByAccountAsync(TransactionFilter filter, CancellationToken cancellationToken);
}
