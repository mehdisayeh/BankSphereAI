using BankSphere.Customers.Domain;
using BankSphere.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSphere.Customers.Infrastructure;

public sealed class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : OutboxDbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customers");
        ConfigureOutbox(modelBuilder, "customers");
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        BankingMapping.Conventions(modelBuilder);
    }
}
