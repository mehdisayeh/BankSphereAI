using BankSphere.Accounts.Application;
using BankSphere.Accounts.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Accounts.Infrastructure;

public sealed class AccountRepository(AccountsDbContext context) : IAccountRepository
{
    public async Task AddAsync(BankAccount account, CancellationToken cancellationToken)
    {
        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);
    }
    public Task<BankAccount?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        context.Accounts.AsSplitQuery().SingleOrDefaultAsync(value => value.Id == id, cancellationToken);
}
