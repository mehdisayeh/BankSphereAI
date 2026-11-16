
namespace BankSphere.Transactions.Domain;

public sealed record TransactionEnrichment(Guid Id, string Source, string Type, string ProtectedReference, DateTimeOffset AtUtc);
