using System.Security.Cryptography;
using System.Text.Json;
using BankSphere.SharedKernel;
using BankSphere.Transactions.Domain;

namespace BankSphere.Transactions.Application;

public sealed class IngestSimulationHandler(ITransactionRepository repository, ISimulatorAccountGateway accounts, TimeProvider clock)
{
    private static readonly JsonSerializerOptions FingerprintOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public async Task<IngestionResult> Handle(IngestSimulation command, RequestActor actor, CancellationToken cancellationToken)
    {
        actor.Require("simulator"); ArgumentNullException.ThrowIfNull(command.Record);
        ArgumentNullException.ThrowIfNull(command.RiskSignals); ArgumentNullException.ThrowIfNull(command.Metadata); ArgumentNullException.ThrowIfNull(command.Enrichments);
        // Fingerprint excludes caller-supplied child UUIDs and sorts unordered enrichment collections.
        var canonical = new { command.Record,
            Signals = command.RiskSignals.Select(item => new { item.Code, item.Severity, item.EvidenceReference, item.DetectedAtUtc }).OrderBy(item => item.Code, StringComparer.Ordinal).ThenBy(item => item.EvidenceReference, StringComparer.Ordinal).ToArray(),
            Metadata = command.Metadata.Select(item => new { item.Key, item.Value }).OrderBy(item => item.Key, StringComparer.Ordinal).ToArray(),
            Enrichments = command.Enrichments.Select(item => new { item.Source, item.Type, item.ProtectedReference, item.AtUtc }).OrderBy(item => item.Source, StringComparer.Ordinal).ThenBy(item => item.Type, StringComparer.Ordinal).ToArray() };
        var fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(canonical, FingerprintOptions)));
        var existing = await repository.FindByExternalReferenceAsync(command.Record.Source, command.Record.ExternalReference, cancellationToken);
        if (existing is not null) return Replay(existing, fingerprint);
        Guard.Against(!await accounts.IsOwnedByAsync(command.Record.AccountId, command.Record.CustomerId, cancellationToken), "simulation.ownership", "Account does not belong to the supplied customer.");
        var transaction = BankingTransaction.Ingest(command.Record, fingerprint, command.RiskSignals, command.Metadata, command.Enrichments, clock.GetUtcNow());
        try { await repository.AddAsync(transaction, cancellationToken); }
        catch (BusinessException exception) when (exception.Code == "persistence.duplicate")
        {
            existing = await repository.FindByExternalReferenceAsync(command.Record.Source, command.Record.ExternalReference, cancellationToken);
            if (existing is null) throw;
            return Replay(existing, fingerprint);
        }
        return new(ToDetails(transaction), false);
    }
    private static IngestionResult Replay(BankingTransaction existing, string fingerprint)
    {
        Guard.Against(existing.PayloadFingerprint != fingerprint, "simulation.idempotency_conflict", "External reference was already ingested with different content.", FailureKind.Conflict);
        return new(ToDetails(existing), true);
    }
    private static TransactionDetails ToDetails(BankingTransaction transaction) => new(transaction.Id, transaction.AccountId, transaction.CustomerId,
        transaction.ExternalTransactionReference, transaction.Type, transaction.Status, transaction.Direction, transaction.Amount.Amount, transaction.Amount.Currency.Code,
        transaction.TransactionDateUtc, transaction.Source, transaction.SimulatorBatchId, transaction.SimulationScenarioId, true);
}
