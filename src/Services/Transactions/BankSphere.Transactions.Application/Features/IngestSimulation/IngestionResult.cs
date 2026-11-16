
namespace BankSphere.Transactions.Application;

public sealed record IngestionResult(TransactionDetails Transaction, bool Replayed);
