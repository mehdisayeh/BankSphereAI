using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankSphere.Transactions.Infrastructure;

public sealed class TransactionsDesignFactory : IDesignTimeDbContextFactory<TransactionsDbContext>
{
    public TransactionsDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("BANKSPHERE_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Database=BankSphereAI_Development;Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<TransactionsDbContext>().UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "transactions")).Options;
        return new(options);
    }
}
