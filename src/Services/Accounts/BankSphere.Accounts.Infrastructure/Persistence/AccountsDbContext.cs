using BankSphere.Accounts.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Accounts.Infrastructure;

public sealed class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : OutboxDbContext(options)
{
    public DbSet<BankAccount> Accounts => Set<BankAccount>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("accounts");
        ConfigureOutbox(modelBuilder, "accounts");
        modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
        modelBuilder.ApplyConfiguration(new AccountBalanceProjectionConfiguration());
        BankingMapping.Conventions(modelBuilder);
    }
}
