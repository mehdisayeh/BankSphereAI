using BankSphere.CreditAssessment.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.CreditAssessment.Infrastructure;

public sealed class CreditAssessmentDbContext(DbContextOptions<CreditAssessmentDbContext> options) : OutboxDbContext(options)
{
    public DbSet<CreditApplication> Applications => Set<CreditApplication>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("credit");
        ConfigureOutbox(modelBuilder, "credit");
        modelBuilder.ApplyConfiguration(new CreditApplicationConfiguration());
        BankingMapping.Conventions(modelBuilder);
    }
}
