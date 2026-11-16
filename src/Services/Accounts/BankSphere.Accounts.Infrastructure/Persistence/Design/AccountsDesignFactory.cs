using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankSphere.Accounts.Infrastructure;

public sealed class AccountsDesignFactory : IDesignTimeDbContextFactory<AccountsDbContext>
{
    public AccountsDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("BANKSPHERE_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Database=BankSphereAI_Development;Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<AccountsDbContext>().UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "accounts")).Options;
        return new(options);
    }
}
