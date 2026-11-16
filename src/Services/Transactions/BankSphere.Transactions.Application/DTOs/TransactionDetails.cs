using BankSphere.Transactions.Domain;

namespace BankSphere.Transactions.Application;

public sealed record TransactionDetails(Guid Id, Guid AccountId, Guid CustomerId, string ExternalReference, TransactionType Type,
    TransactionStatus Status, TransactionDirection Direction, decimal Amount, string Currency, DateTimeOffset TransactionDateUtc,
    string Source, string SimulatorBatchId, string SimulationScenarioId, bool IsSynthetic);
