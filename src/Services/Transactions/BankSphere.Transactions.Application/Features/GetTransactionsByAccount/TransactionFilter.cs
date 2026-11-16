using BankSphere.SharedKernel;
using BankSphere.Transactions.Domain;

namespace BankSphere.Transactions.Application;

public sealed record TransactionFilter(Guid AccountId, PageRequest Page, TransactionStatus? Status = null,
    DateTimeOffset? FromUtc = null, DateTimeOffset? ToUtc = null);
