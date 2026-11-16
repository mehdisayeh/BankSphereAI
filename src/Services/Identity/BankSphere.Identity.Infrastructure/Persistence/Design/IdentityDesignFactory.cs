using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankSphere.Identity.Infrastructure;

public sealed class IdentityDesignFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("BANKSPHERE_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Database=BankSphereAI_Development;Integrated Security=true;TrustServerCertificate=true";
        return new(new DbContextOptionsBuilder<IdentityDbContext>().UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")).UseOpenIddict().Options);
    }
}
