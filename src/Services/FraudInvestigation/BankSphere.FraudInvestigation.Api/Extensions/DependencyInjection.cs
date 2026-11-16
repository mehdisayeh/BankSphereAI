using BankSphere.FraudInvestigation.Application;
using BankSphere.FraudInvestigation.Infrastructure;
using BankSphere.Security;
using BankSphere.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BankSphere.FraudInvestigation.Api.Extensions;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddBankingServices(this WebApplicationBuilder builder)
    {
        builder.AddBankingApi();
        var connection = builder.Configuration.GetConnectionString("Banking")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Banking for the banking API.");
        builder.Services.AddDbContext<FraudInvestigationDbContext>(options => options.UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "fraudinvestigation")));
        builder.Services.AddScoped<IFraudCaseRepository, FraudCaseRepository>();
        builder.Services.AddScoped<OpenFraudInvestigationHandler>();
        builder.Services.AddScoped<GetFraudInvestigationHandler>();
        builder.Services.AddScoped<RequestFraudReviewHandler>();
        builder.Services.AddScoped<FraudEvidenceHandler>();
        builder.Services.AddScoped<RecordFraudDecisionHandler>();
        builder.Services.AddHttpClient<IInvestigationTransactions, InvestigationTransactions>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:Transactions"] ?? "http://localhost:5103");
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<BearerForwardingHandler>();
        builder.Services.AddHealthChecks().AddAsyncCheck("sql", async cancellationToken =>
        {
            await using var database = new FraudInvestigationDbContext(new DbContextOptionsBuilder<FraudInvestigationDbContext>().UseSqlServer(connection).Options);
            return await database.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        });
        return builder;
    }
}
