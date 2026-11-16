using BankSphere.Transactions.Application;
using BankSphere.Transactions.Infrastructure;
using BankSphere.Security;
using BankSphere.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BankSphere.Transactions.Api.Extensions;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddBankingServices(this WebApplicationBuilder builder)
    {
        builder.AddBankingApi();
        var connection = builder.Configuration.GetConnectionString("Banking")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Banking for the banking API.");
        builder.Services.AddDbContext<TransactionsDbContext>(options => options.UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "transactions")));
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<IngestSimulationHandler>();
        builder.Services.AddScoped<GetTransactionsByAccountHandler>();
        builder.Services.AddHttpClient<ISimulatorAccountGateway, SimulatorAccountGateway>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:Accounts"] ?? "http://localhost:5102");
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<BearerForwardingHandler>();
        builder.Services.AddHealthChecks().AddAsyncCheck("sql", async cancellationToken =>
        {
            await using var database = new TransactionsDbContext(new DbContextOptionsBuilder<TransactionsDbContext>().UseSqlServer(connection).Options);
            return await database.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        });
        builder.Services.AddScoped<GetTransactionByIdHandler>();
        return builder;
    }
}
