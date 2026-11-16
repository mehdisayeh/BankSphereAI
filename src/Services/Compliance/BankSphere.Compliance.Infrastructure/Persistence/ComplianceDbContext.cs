using BankSphere.Compliance.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Compliance.Infrastructure;

public sealed class ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : OutboxDbContext(options)
{
    public DbSet<ComplianceCase> Cases => Set<ComplianceCase>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("compliance");
        ConfigureOutbox(modelBuilder, "compliance");
        modelBuilder.ApplyConfiguration(new ComplianceCaseConfiguration());
        BankingMapping.Conventions(modelBuilder);
    }
}
