using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankSphere.FraudInvestigation.Infrastructure;

public sealed class FraudInvestigationDesignFactory : IDesignTimeDbContextFactory<FraudInvestigationDbContext>
{
    public FraudInvestigationDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("BANKSPHERE_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Database=BankSphereAI_Development;Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<FraudInvestigationDbContext>().UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "fraudinvestigation")).Options;
        return new(options);
    }
}
