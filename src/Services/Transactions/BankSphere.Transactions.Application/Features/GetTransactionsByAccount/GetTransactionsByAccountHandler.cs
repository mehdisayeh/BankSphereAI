using BankSphere.SharedKernel;

namespace BankSphere.Transactions.Application;

public sealed class GetTransactionsByAccountHandler(ITransactionRepository repository)
{
    public Task<Page<TransactionDetails>> Handle(TransactionFilter filter, CancellationToken cancellationToken)
    {
        Guard.Id(filter.AccountId, "Account"); filter.Page.Validate();
        if (filter.Status is not null) Guard.Defined(filter.Status.Value);
        if (filter.FromUtc is not null) Guard.Utc(filter.FromUtc.Value);
        if (filter.ToUtc is not null) Guard.Utc(filter.ToUtc.Value);
        Guard.Against(filter.FromUtc > filter.ToUtc, "transaction.date_filter", "From must precede to.");
        return repository.ByAccountAsync(filter, cancellationToken);
    }
}
