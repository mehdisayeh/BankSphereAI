using BankSphere.SharedKernel;

namespace BankSphere.Transactions.Domain;

public sealed class BankingTransaction : BankingAggregate
{
    private readonly List<TransactionMetadata> metadata = [];
    private readonly List<TransactionStatusHistory> statusHistory = [];
    private readonly List<TransactionRiskSignal> riskSignals = [];
    private readonly List<TransactionEnrichment> enrichments = [];
    private BankingTransaction() { }
    public string Source { get; private set; } = "";
    public string ExternalTransactionReference { get; private set; } = "";
    public Guid AccountId { get; private set; }
    public Guid CustomerId { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public TransactionDirection Direction { get; private set; }
    public Money Amount { get; private set; } = null!;
    public Money OriginalAmount { get; private set; } = null!;
    public string? ExchangeRateReference { get; private set; }
    public DateTimeOffset TransactionDateUtc { get; private set; }
    public DateTimeOffset? BookingDateUtc { get; private set; }
    public DateOnly ValueDate { get; private set; }
    public MerchantDetails? Merchant { get; private set; }
    public CounterpartyDetails? Counterparty { get; private set; }
    public PaymentInstrumentReference Instrument { get; private set; } = null!;
    public string PaymentChannel { get; private set; } = "";
    public string PaymentMethod { get; private set; } = "";
    public string PaymentScheme { get; private set; } = "";
    public string Description { get; private set; } = "";
    public string? RemittanceInformation { get; private set; }
    public string? TerminalId { get; private set; }
    public string? DeviceReference { get; private set; }
    public string? IpAddressReference { get; private set; }
    public string? GeoLocationReference { get; private set; }
    public string InitiationChannel { get; private set; } = "";
    public string SimulatorBatchId { get; private set; } = "";
    public string SimulationScenarioId { get; private set; } = "";
    public bool IsSynthetic { get; private set; } = true;
    public string PayloadFingerprint { get; private set; } = "";
    public IReadOnlyCollection<TransactionMetadata> Metadata => metadata.AsReadOnly();
    public IReadOnlyCollection<TransactionStatusHistory> StatusHistory => statusHistory.AsReadOnly();
    public IReadOnlyCollection<TransactionRiskSignal> RiskSignals => riskSignals.AsReadOnly();
    public IReadOnlyCollection<TransactionEnrichment> Enrichments => enrichments.AsReadOnly();

    public static BankingTransaction Ingest(SimulatorRecord record, string fingerprint, IReadOnlyList<TransactionRiskSignal> signals,
        IReadOnlyList<TransactionMetadata> metadata, IReadOnlyList<TransactionEnrichment> enrichments, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(record);
        Guard.Against(record.Amount.Amount <= 0, "transaction.amount", "Synthetic transaction amount must be positive.");
        Guard.Against(record.Amount.Currency != record.OriginalAmount.Currency && string.IsNullOrWhiteSpace(record.ExchangeRateReference), "transaction.fx", "Cross-currency records require an FX reference.");
        Guard.Against(record.Amount.Currency == record.OriginalAmount.Currency && record.Amount != record.OriginalAmount, "transaction.original_amount", "Same-currency original amount must match.");
        Guard.Against(record.TransactionDateUtc > now || record.ValueDate == default, "transaction.dates", "Transaction date cannot be in the future and value date is required.");
        if (record.BookingDateUtc is not null) Guard.Utc(record.BookingDateUtc.Value);
        Guard.Against(record.Status == TransactionStatus.Booked && record.BookingDateUtc is null, "transaction.booking", "Booked simulator record requires booking timestamp.");
        Guard.Against(record.BookingDateUtc < record.TransactionDateUtc || record.BookingDateUtc > now, "transaction.booking", "Invalid booking timestamp.");
        var transaction = new BankingTransaction { Source = Guard.Text(record.Source, "Simulator source", 100),
            ExternalTransactionReference = Guard.Text(record.ExternalReference, "External reference", 100), AccountId = Guard.Id(record.AccountId, "Account"), CustomerId = Guard.Id(record.CustomerId, "Customer"),
            Type = Guard.Defined(record.Type), Status = Guard.Defined(record.Status), Direction = Guard.Defined(record.Direction), Amount = record.Amount,
            OriginalAmount = record.OriginalAmount, ExchangeRateReference = record.ExchangeRateReference, TransactionDateUtc = Guard.Utc(record.TransactionDateUtc), BookingDateUtc = record.BookingDateUtc, ValueDate = record.ValueDate,
            Merchant = record.Merchant is null ? null : record.Merchant with { Name = Guard.Text(record.Merchant.Name, "Merchant"), Country = Guard.Country(record.Merchant.Country), CategoryCode = Guard.Text(record.Merchant.CategoryCode, "MCC", 4) },
            Counterparty = record.Counterparty, Instrument = new(Guard.Text(record.Instrument.Type, "Instrument"), Guard.Text(record.Instrument.ProtectedReference, "Protected instrument reference")),
            PaymentChannel = Guard.Text(record.PaymentChannel, "Payment channel"), PaymentMethod = Guard.Text(record.PaymentMethod, "Payment method"), PaymentScheme = Guard.Text(record.PaymentScheme, "Scheme"),
            Description = Guard.Text(record.Description, "Description", 2000), RemittanceInformation = record.RemittanceInformation, TerminalId = record.TerminalId,
            DeviceReference = record.DeviceReference, IpAddressReference = record.IpAddressReference, GeoLocationReference = record.GeoLocationReference,
            InitiationChannel = Guard.Text(record.InitiationChannel, "Initiation channel"), SimulatorBatchId = Guard.Text(record.SimulatorBatchId, "Batch"),
            SimulationScenarioId = Guard.Text(record.SimulationScenarioId, "Scenario"), PayloadFingerprint = Guard.Text(fingerprint, "Fingerprint", 64) };
        transaction.Initialize(now);
        foreach (var signal in signals) transaction.riskSignals.Add(new(Guid.NewGuid(), Guard.Text(signal.Code, "Signal"), Guard.Text(signal.Severity, "Severity"), Guard.Text(signal.EvidenceReference, "Evidence"), Guard.Utc(signal.DetectedAtUtc)));
        foreach (var item in metadata) transaction.metadata.Add(new(Guid.NewGuid(), Guard.Text(item.Key, "Metadata key"), Guard.Text(item.Value, "Metadata value")));
        foreach (var item in enrichments) transaction.enrichments.Add(new(Guid.NewGuid(), Guard.Text(item.Source, "Enrichment source"), Guard.Text(item.Type, "Enrichment type"), Guard.Text(item.ProtectedReference, "Reference"), Guard.Utc(item.AtUtc)));
        transaction.statusHistory.Add(new(Guid.NewGuid(), record.Status, record.ExternalReference, now));
        transaction.RaiseDomainEvent(new TransactionCreated(transaction.Id, transaction.AccountId, transaction.CustomerId, transaction.Amount.Amount,
            transaction.Amount.Currency.Code, transaction.Source, transaction.SimulatorBatchId, transaction.SimulationScenarioId, 1, Guid.NewGuid(), now));
        return transaction;
    }
}
