using BankSphere.Accounts.Application;
using BankSphere.Accounts.Infrastructure;
using BankSphere.Security;
using BankSphere.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BankSphere.Accounts.Api.Extensions;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddBankingServices(this WebApplicationBuilder builder)
    {
        builder.AddBankingApi();
        var connection = builder.Configuration.GetConnectionString("Banking")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Banking for the banking API.");
        builder.Services.AddDbContext<AccountsDbContext>(options => options.UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "accounts")));
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<OpenAccountHandler>();
        builder.Services.AddScoped<GetAccountByIdHandler>();
        builder.Services.AddHttpClient<ICustomerEligibilityGateway, CustomerEligibilityGateway>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:Customers"] ?? "http://localhost:5101");
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<BearerForwardingHandler>();
        builder.Services.AddHealthChecks().AddAsyncCheck("sql", async cancellationToken =>
        {
            await using var database = new AccountsDbContext(new DbContextOptionsBuilder<AccountsDbContext>().UseSqlServer(connection).Options);
            return await database.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        });
        return builder;
    }
}
