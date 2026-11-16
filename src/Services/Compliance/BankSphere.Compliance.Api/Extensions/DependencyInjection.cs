using BankSphere.Compliance.Application;
using BankSphere.Compliance.Infrastructure;
using BankSphere.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BankSphere.Compliance.Api.Extensions;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddBankingServices(this WebApplicationBuilder builder)
    {
        builder.AddBankingApi();
        var connection = builder.Configuration.GetConnectionString("Banking")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Banking for the banking API.");
        builder.Services.AddDbContext<ComplianceDbContext>(options => options.UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "compliance")));
        builder.Services.AddScoped<IComplianceRepository, ComplianceRepository>();
        builder.Services.AddScoped<OpenComplianceCaseHandler>();
        builder.Services.AddScoped<GetComplianceCaseHandler>();
        builder.Services.AddHealthChecks().AddAsyncCheck("sql", async cancellationToken =>
        {
            await using var database = new ComplianceDbContext(new DbContextOptionsBuilder<ComplianceDbContext>().UseSqlServer(connection).Options);
            return await database.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        });
        return builder;
    }
}
