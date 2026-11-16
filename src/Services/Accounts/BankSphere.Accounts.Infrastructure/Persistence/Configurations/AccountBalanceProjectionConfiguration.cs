using BankSphere.Accounts.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankSphere.Accounts.Infrastructure;

public sealed class AccountBalanceProjectionConfiguration : IEntityTypeConfiguration<AccountBalanceProjection>
{
    public void Configure(EntityTypeBuilder<AccountBalanceProjection> builder)
    {
        builder.ToTable("BalanceSnapshots", "accounts");
        builder.HasKey(value => value.AccountId);
        builder.HasOne<BankAccount>().WithOne().HasForeignKey<AccountBalanceProjection>(value => value.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.OwnsOne(value => value.Snapshot, snapshot =>
        {
            BankingMapping.Money(snapshot.OwnsOne(value => value.BookedBalance));
            BankingMapping.Money(snapshot.OwnsOne(value => value.AvailableBalance));
            BankingMapping.Money(snapshot.OwnsOne(value => value.PendingAmount));
            snapshot.Navigation(value => value.BookedBalance).IsRequired();
            snapshot.Navigation(value => value.AvailableBalance).IsRequired();
            snapshot.Navigation(value => value.PendingAmount).IsRequired();
        });
        builder.Navigation(value => value.Snapshot).IsRequired();
    }
}
