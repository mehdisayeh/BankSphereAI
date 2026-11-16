using BankSphere.Transactions.Domain;

namespace BankSphere.Transactions.Application;

public sealed record IngestSimulation(SimulatorRecord Record, IReadOnlyList<TransactionRiskSignal> RiskSignals,
    IReadOnlyList<TransactionMetadata> Metadata, IReadOnlyList<TransactionEnrichment> Enrichments);
