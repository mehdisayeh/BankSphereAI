using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankSphere.CreditAssessment.Infrastructure;

public sealed class CreditAssessmentDesignFactory : IDesignTimeDbContextFactory<CreditAssessmentDbContext>
{
    public CreditAssessmentDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("BANKSPHERE_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Database=BankSphereAI_Development;Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<CreditAssessmentDbContext>().UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "credit")).Options;
        return new(options);
    }
}
