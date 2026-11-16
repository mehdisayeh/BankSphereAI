
namespace BankSphere.Transactions.Domain;

public sealed record TransactionStatusHistory(Guid Id, TransactionStatus Status, string SourceReference, DateTimeOffset AtUtc);
