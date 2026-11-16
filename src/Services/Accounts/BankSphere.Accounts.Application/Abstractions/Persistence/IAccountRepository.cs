using BankSphere.Accounts.Domain;

namespace BankSphere.Accounts.Application;

public interface IAccountRepository
{
    Task AddAsync(BankAccount account, CancellationToken cancellationToken);
    Task<BankAccount?> FindAsync(Guid id, CancellationToken cancellationToken);
}
