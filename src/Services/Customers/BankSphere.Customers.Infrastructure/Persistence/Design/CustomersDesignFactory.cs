using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankSphere.Customers.Infrastructure;

public sealed class CustomersDesignFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public CustomersDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("BANKSPHERE_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Database=BankSphereAI_Development;Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<CustomersDbContext>().UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "customers")).Options;
        return new(options);
    }
}
