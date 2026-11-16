using BankSphere.Accounts.Domain;
using BankSphere.Persistence;
using BankSphere.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Accounts.Infrastructure;

public sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        BankingMapping.Root(builder, "accounts", "Accounts");
        builder.Property(value => value.AccountNumber).HasMaxLength(40);
        builder.HasIndex(value => value.AccountNumber).IsUnique();
        builder.Property(value => value.Currency).HasConversion(value => value.Code, code => new Currency(code)).HasColumnType("char(3)");
        builder.Property(value => value.Iban).HasConversion(value => value == null ? null : value.Value, value => value == null ? null : new Iban(value)).HasMaxLength(34);
        builder.HasIndex(value => value.Iban).IsUnique().HasFilter("[Iban] IS NOT NULL");
        builder.HasIndex(value => value.PrimaryOwnerId);
        BankingMapping.Money(builder.OwnsOne(value => value.DailyTransactionLimit));
        builder.Navigation(value => value.DailyTransactionLimit).IsRequired();
        BankingMapping.Money(builder.OwnsOne(value => value.MonthlyTransactionLimit));
        builder.Navigation(value => value.MonthlyTransactionLimit).IsRequired();
        builder.OwnsMany(value => value.Ownerships, owned =>
        {
            BankingMapping.Child(owned, "accounts", "BankAccountOwnerships");
        });
        builder.OwnsMany(value => value.Mandates, owned =>
        {
            BankingMapping.Child(owned, "accounts", "BankAccountMandates");
        });
        builder.OwnsMany(value => value.Restrictions, owned =>
        {
            BankingMapping.Child(owned, "accounts", "BankAccountRestrictions");
        });
        builder.OwnsMany(value => value.StatusHistory, owned =>
        {
            BankingMapping.Child(owned, "accounts", "BankAccountStatusHistory");
        });
    }
}
