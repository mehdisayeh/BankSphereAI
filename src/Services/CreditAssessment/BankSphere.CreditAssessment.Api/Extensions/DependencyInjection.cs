using BankSphere.CreditAssessment.Application;
using BankSphere.CreditAssessment.Infrastructure;
using BankSphere.Security;
using BankSphere.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BankSphere.CreditAssessment.Api.Extensions;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddBankingServices(this WebApplicationBuilder builder)
    {
        builder.AddBankingApi();
        var connection = builder.Configuration.GetConnectionString("Banking")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Banking for the banking API.");
        builder.Services.AddDbContext<CreditAssessmentDbContext>(options => options.UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "credit")));
        builder.Services.AddScoped<ICreditApplicationRepository, CreditApplicationRepository>();
        builder.Services.AddScoped<SubmitCreditApplicationHandler>();
        builder.Services.AddScoped<GetCreditApplicationStatusHandler>();
        builder.Services.AddHttpClient<ICreditCustomerGateway, CreditCustomerGateway>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:Customers"] ?? "http://localhost:5101");
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<BearerForwardingHandler>();
        builder.Services.AddHealthChecks().AddAsyncCheck("sql", async cancellationToken =>
        {
            await using var database = new CreditAssessmentDbContext(new DbContextOptionsBuilder<CreditAssessmentDbContext>().UseSqlServer(connection).Options);
            return await database.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        });
        return builder;
    }
}
