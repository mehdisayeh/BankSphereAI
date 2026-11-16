using BankSphere.FraudInvestigation.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.FraudInvestigation.Infrastructure;

public sealed class FraudInvestigationDbContext(DbContextOptions<FraudInvestigationDbContext> options) : OutboxDbContext(options)
{
    public DbSet<FraudCase> Cases => Set<FraudCase>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("fraudinvestigation");
        ConfigureOutbox(modelBuilder, "fraudinvestigation");
        modelBuilder.ApplyConfiguration(new FraudCaseConfiguration());
        BankingMapping.Conventions(modelBuilder);
    }
}
