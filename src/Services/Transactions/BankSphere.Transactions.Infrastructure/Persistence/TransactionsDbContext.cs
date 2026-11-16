using BankSphere.Transactions.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Transactions.Infrastructure;

public sealed class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : OutboxDbContext(options)
{
    public DbSet<BankingTransaction> Transactions => Set<BankingTransaction>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("transactions");
        ConfigureOutbox(modelBuilder, "transactions");
        modelBuilder.ApplyConfiguration(new BankingTransactionConfiguration());
        BankingMapping.Conventions(modelBuilder);
    }
}
