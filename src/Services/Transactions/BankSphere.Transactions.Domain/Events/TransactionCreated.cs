using BankSphere.SharedKernel;

namespace BankSphere.Transactions.Domain;

// Integration contract: references only, never merchant/contact/identity data.
public sealed record TransactionCreated(Guid TransactionId, Guid AccountId, Guid CustomerId,
    decimal Amount, string Currency, string Source, string SimulatorBatchId, string SimulationScenarioId,
    int SchemaVersion, Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
