using BankSphere.SharedKernel;

namespace BankSphere.Transactions.Domain;

public sealed record SimulatorRecord(string Source, string ExternalReference, Guid AccountId, Guid CustomerId,
    TransactionType Type, TransactionStatus Status, TransactionDirection Direction, Money Amount, Money OriginalAmount,
    string? ExchangeRateReference, DateTimeOffset TransactionDateUtc, DateTimeOffset? BookingDateUtc, DateOnly ValueDate,
    MerchantDetails? Merchant, CounterpartyDetails? Counterparty, PaymentInstrumentReference Instrument,
    string PaymentChannel, string PaymentMethod, string PaymentScheme, string Description, string? RemittanceInformation,
    string? TerminalId, string? DeviceReference, string? IpAddressReference, string? GeoLocationReference,
    string InitiationChannel, string SimulatorBatchId, string SimulationScenarioId);
