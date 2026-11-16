using BankSphere.Accounts.Domain;
using BankSphere.Accounts.Infrastructure;
using BankSphere.Compliance.Domain;
using BankSphere.Compliance.Infrastructure;
using BankSphere.CreditAssessment.Domain;
using BankSphere.CreditAssessment.Infrastructure;
using BankSphere.Customers.Domain;
using BankSphere.Customers.Infrastructure;
using BankSphere.FraudInvestigation.Domain;
using BankSphere.FraudInvestigation.Infrastructure;
using BankSphere.Persistence;
using BankSphere.SharedKernel;
using BankSphere.Transactions.Application;
using BankSphere.Transactions.Domain;
using BankSphere.Transactions.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankSphere.Persistence.Tests;

public sealed class SqlDatabase : IAsyncLifetime
{
    private readonly string database = $"BankSphereAI_Tests_{Guid.NewGuid():N}";
    public string Connection { get; }
    public SqlDatabase()
    {
        var builder = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("BANKSPHERE_TEST_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Integrated Security=true;TrustServerCertificate=true");
        builder.InitialCatalog = database; Connection = builder.ConnectionString;
    }
    public DbContextOptions<T> Options<T>(string schema) where T : DbContext => new DbContextOptionsBuilder<T>()
        .UseSqlServer(Connection, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema)).Options;
    public CustomersDbContext Customers() => new(Options<CustomersDbContext>("customers"));
    public AccountsDbContext Accounts() => new(Options<AccountsDbContext>("accounts"));
    public TransactionsDbContext Transactions() => new(Options<TransactionsDbContext>("transactions"));
    public FraudInvestigationDbContext Fraud() => new(Options<FraudInvestigationDbContext>("fraudinvestigation"));
    public ComplianceDbContext Compliance() => new(Options<ComplianceDbContext>("compliance"));
    public CreditAssessmentDbContext Credit() => new(Options<CreditAssessmentDbContext>("credit"));
    public async Task InitializeAsync()
    {
        DbContext[] contexts = [Customers(), Accounts(), Transactions(), Fraud(), Compliance(), Credit()];
        try { foreach (var context in contexts) await context.Database.MigrateAsync(); }
        finally { foreach (var context in contexts) await context.DisposeAsync(); }
    }
    public async Task DisposeAsync()
    {
        var actual = new SqlConnectionStringBuilder(Connection).InitialCatalog;
        if (actual != database || !actual.StartsWith("BankSphereAI_Tests_", StringComparison.Ordinal)) throw new InvalidOperationException("Test database boundary check failed.");
        await using var context = Customers(); await context.Database.EnsureDeletedAsync();
    }
}
