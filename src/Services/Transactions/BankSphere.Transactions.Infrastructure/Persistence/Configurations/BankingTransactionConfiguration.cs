using BankSphere.Transactions.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Transactions.Infrastructure;

public sealed class BankingTransactionConfiguration : IEntityTypeConfiguration<BankingTransaction>
{
    public void Configure(EntityTypeBuilder<BankingTransaction> builder)
    {
        BankingMapping.Root(builder, "transactions", "Transactions");
        builder.Property(value => value.Source).HasMaxLength(100);
        builder.Property(value => value.ExternalTransactionReference).HasMaxLength(100);
        builder.Property(value => value.PayloadFingerprint).HasMaxLength(64);
        builder.HasIndex(value => new { value.Source, value.ExternalTransactionReference }).IsUnique();
        builder.HasIndex(value => new { value.AccountId, value.TransactionDateUtc, value.Id });
        builder.ToTable("Transactions", "transactions", table => table.HasCheckConstraint("CK_Transactions_Synthetic", "[IsSynthetic] = 1"));
        BankingMapping.Money(builder.OwnsOne(value => value.Amount));
        builder.Navigation(value => value.Amount).IsRequired();
        BankingMapping.Money(builder.OwnsOne(value => value.OriginalAmount));
        builder.Navigation(value => value.OriginalAmount).IsRequired();
        builder.OwnsOne(value => value.Merchant, owned =>
        {
            owned.ToTable("BankingTransactionMerchant", "transactions");
        });
        builder.OwnsOne(value => value.Counterparty, owned =>
        {
            owned.ToTable("BankingTransactionCounterparty", "transactions");
        });
        builder.OwnsOne(value => value.Instrument, owned =>
        {
            owned.ToTable("BankingTransactionInstrument", "transactions");
        });
        builder.OwnsMany(value => value.Metadata, owned =>
        {
            BankingMapping.Child(owned, "transactions", "BankingTransactionMetadata");
        });
        builder.OwnsMany(value => value.StatusHistory, owned =>
        {
            BankingMapping.Child(owned, "transactions", "BankingTransactionStatusHistory");
        });
        builder.OwnsMany(value => value.RiskSignals, owned =>
        {
            BankingMapping.Child(owned, "transactions", "BankingTransactionRiskSignals");
        });
        builder.OwnsMany(value => value.Enrichments, owned =>
        {
            BankingMapping.Child(owned, "transactions", "BankingTransactionEnrichments");
        });
    }
}
