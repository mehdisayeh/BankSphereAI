using BankSphere.Transactions.Application;
using BankSphere.Transactions.Domain;
using BankSphere.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Transactions.Infrastructure;

public sealed class TransactionRepository(TransactionsDbContext context) : ITransactionRepository
{
    public async Task AddAsync(BankingTransaction transaction, CancellationToken cancellationToken)
    {
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);
    }
    public Task<BankingTransaction?> FindByExternalReferenceAsync(string source, string reference, CancellationToken cancellationToken) =>
        context.Transactions.AsNoTracking().AsSplitQuery().SingleOrDefaultAsync(value => value.Source == source && value.ExternalTransactionReference == reference, cancellationToken);
    public Task<TransactionDetails?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        Details(context.Transactions.Where(value => value.Id == id)).SingleOrDefaultAsync(cancellationToken);
    private static IQueryable<TransactionDetails> Details(IQueryable<BankingTransaction> query) => query.AsNoTracking().Select(value =>
        new TransactionDetails(value.Id, value.AccountId, value.CustomerId, value.ExternalTransactionReference, value.Type, value.Status,
            value.Direction, value.Amount.Amount, value.Amount.Currency.Code, value.TransactionDateUtc, value.Source, value.SimulatorBatchId, value.SimulationScenarioId, value.IsSynthetic));
    public async Task<Page<TransactionDetails>> ByAccountAsync(TransactionFilter filter, CancellationToken cancellationToken)
    {
        var query = context.Transactions.AsNoTracking().Where(value => value.AccountId == filter.AccountId);
        if (filter.Status is not null) query = query.Where(value => value.Status == filter.Status);
        if (filter.FromUtc is not null) query = query.Where(value => value.TransactionDateUtc >= filter.FromUtc);
        if (filter.ToUtc is not null) query = query.Where(value => value.TransactionDateUtc <= filter.ToUtc);
        var count = await query.CountAsync(cancellationToken);
        IOrderedQueryable<BankingTransaction> ordered = filter.Page.Sort == "amount"
            ? (filter.Page.Descending ? query.OrderByDescending(value => value.Amount.Amount) : query.OrderBy(value => value.Amount.Amount))
            : (filter.Page.Descending ? query.OrderByDescending(value => value.TransactionDateUtc) : query.OrderBy(value => value.TransactionDateUtc));
        var items = await Details(ordered.ThenBy(value => value.Id).Skip((filter.Page.PageNumber - 1) * filter.Page.PageSize)
            .Take(filter.Page.PageSize)).ToListAsync(cancellationToken);
        return new(items, filter.Page.PageNumber, filter.Page.PageSize, count);
    }
}
