using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using BankSphere.Identity.Api;
using BankSphere.Identity.Api.Extensions;
using BankSphere.Identity.Application;
using BankSphere.Identity.Infrastructure;
using BankSphere.Security;
using BankSphere.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace BankSphere.Identity.IntegrationTests;

public sealed class IdentityFixture : IAsyncLifetime
{
    private readonly string databaseName = "BankSphereAI_IdentityTests_" + Guid.NewGuid().ToString("N");
    private readonly string directory = Path.Combine(Path.GetTempPath(), "BankSphereIdentityTests", Guid.NewGuid().ToString("N"));
    private readonly string certificatePassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private X509Certificate2 signing = null!;
    private X509Certificate2 encryption = null!;
    public string Connection { get; private set; } = "";
    public WebApplication App { get; private set; } = null!;
    public CapturingNotifications Notifications { get; } = new();
    public string CustomerRoute { get; private set; } = "";
    public string AccountRoute { get; private set; } = "";
    public string TransactionsRoute { get; private set; } = "";
    public string FraudRoute { get; private set; } = "";
    public string ComplianceRoute { get; private set; } = "";
    public string CreditRoute { get; private set; } = "";

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(directory);
        signing = Certificate("Identity signing"); encryption = Certificate("Identity encryption");
        await File.WriteAllBytesAsync(Path.Combine(directory, "signing.pfx"), signing.Export(X509ContentType.Pfx, certificatePassword));
        await File.WriteAllBytesAsync(Path.Combine(directory, "encryption.pfx"), encryption.Export(X509ContentType.Pfx, certificatePassword));
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("BANKSPHERE_TEST_SQL_CONNECTION")
            ?? @"Server=.\SQLEXPRESS;Integrated Security=true;TrustServerCertificate=true") { InitialCatalog = databaseName };
        Connection = connection.ConnectionString;
        await using (var context = new IdentityDbContext(new DbContextOptionsBuilder<IdentityDbContext>().UseSqlServer(Connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")).UseOpenIddict().Options)) await context.Database.MigrateAsync();
        App = await CreateIdentityAppAsync();
        await SeedBankingAsync();
    }

    public async Task<WebApplication> CreateIdentityAppAsync(int permitLimit = 300)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(IdentityApiMarker).Assembly.GetName().Name });
        builder.WebHost.UseTestServer(); builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Banking"] = Connection, ["Authentication:Authority"] = "https://identity.test/",
            ["AllowedHosts"] = "*", ["Identity:SigningCertificate:Path"] = Path.Combine(directory, "signing.pfx"),
            ["Identity:SigningCertificate:Password"] = certificatePassword,
            ["Identity:EncryptionCertificate:Path"] = Path.Combine(directory, "encryption.pfx"),
            ["Identity:EncryptionCertificate:Password"] = certificatePassword,
            ["Identity:DataProtection:Directory"] = Path.Combine(directory, "keys"),
            ["Identity:Clients:0:ClientId"] = "integration-client",
            ["Identity:Clients:0:RedirectUris:0"] = "https://client.test/callback",
            ["Identity:RateLimit:PermitLimit"] = permitLimit.ToString(System.Globalization.CultureInfo.InvariantCulture)
        });
        builder.AddIdentityServices();
        builder.Services.RemoveAll<IIdentityNotificationSender>(); builder.Services.AddSingleton<IIdentityNotificationSender>(Notifications);
        builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var metadata = new OpenIdConnectConfiguration { Issuer = "https://identity.test/" };
            metadata.SigningKeys.Add(new X509SecurityKey(signing));
            options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(metadata);
        });
        var app = builder.Build(); app.UseIdentityApi(); app.MapIdentityEndpoints();
        await app.StartAsync(); return app;
    }

    public HttpClient Browser() => new(new BrowserCookies(App.GetTestServer().CreateHandler())) { BaseAddress = new Uri("https://identity.test/") };
    public HttpClient Client(string? accessToken = null)
    {
        var client = App.GetTestClient(); client.BaseAddress = new Uri("https://identity.test/");
        if (accessToken is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static async Task AddCsrfAsync(HttpClient browser)
    {
        var token = (await browser.GetFromJsonAsync<JsonElement>("/identity/antiforgery")).GetProperty("requestToken").GetString();
        browser.DefaultRequestHeaders.Remove("X-CSRF-TOKEN"); browser.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token);
    }

    public async Task<(ApplicationUser User, string Password)> UserAsync(params string[] roles)
    {
        using var scope = App.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = Guid.NewGuid().ToString("N") + "@example.invalid";
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = email, UserName = email, EmailConfirmed = true,
            DisplayName = "Synthetic identity", LockoutEnabled = true, CreatedAtUtc = DateTimeOffset.UtcNow };
        var password = Password();
        Assert.True((await manager.CreateAsync(user, password)).Succeeded);
        if (roles.Length > 0) Assert.True((await manager.AddToRolesAsync(user, roles)).Succeeded);
        return (user, password);
    }
    public static string Password() => "A7!z" + WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    public async Task<JsonElement> TokensAsync(params string[] roles)
    {
        var (user, password) = await UserAsync(roles);
        using var browser = Browser(); await AddCsrfAsync(browser);
        using var login = await browser.PostAsJsonAsync("/identity/login", new { user.Email, password });
        Assert.Equal(System.Net.HttpStatusCode.NoContent, login.StatusCode);
        var (code, verifier) = await AuthorizeAsync(browser);
        using var response = await browser.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code", ["client_id"] = "integration-client", ["code"] = code,
            ["code_verifier"] = verifier, ["redirect_uri"] = "https://client.test/callback"
        }));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(response.IsSuccessStatusCode, json.ToString()); return json;
    }
    public static async Task<(string Code, string Verifier)> AuthorizeAsync(HttpClient browser, string challengeMethod = "S256")
    {
        var verifier = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var challenge = WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        var path = QueryHelpers.AddQueryString("/connect/authorize", new Dictionary<string, string?>
        {
            ["response_type"] = "code", ["client_id"] = "integration-client", ["redirect_uri"] = "https://client.test/callback",
            ["scope"] = "openid profile banking offline_access", ["state"] = Guid.NewGuid().ToString("N"),
            ["code_challenge"] = challenge, ["code_challenge_method"] = challengeMethod
        });
        using var response = await browser.GetAsync(path);
        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        var values = QueryHelpers.ParseQuery(response.Headers.Location!.Query);
        Assert.True(values.ContainsKey("code"), response.Headers.Location.ToString());
        return (values["code"].ToString(), verifier);
    }
    public async Task<HttpResponseMessage> RefreshAsync(string refreshToken)
    {
        using var client = Client();
        return await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
            { ["grant_type"] = "refresh_token", ["client_id"] = "integration-client", ["refresh_token"] = refreshToken }));
    }

    public async Task<WebApplication> BankingAppAsync(string context, bool sessionUnavailable = false)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer(); builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            { ["ConnectionStrings:Banking"] = Connection, ["Authentication:Authority"] = "https://identity.test/", ["AllowedHosts"] = "*" });
        Action<WebApplication> map;
        switch (context)
        {
            case "Customers": BankSphere.Customers.Api.Extensions.DependencyInjection.AddBankingServices(builder); map = BankSphere.Customers.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints; break;
            case "Accounts": BankSphere.Accounts.Api.Extensions.DependencyInjection.AddBankingServices(builder); map = BankSphere.Accounts.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints; break;
            case "Transactions": BankSphere.Transactions.Api.Extensions.DependencyInjection.AddBankingServices(builder); map = BankSphere.Transactions.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints; break;
            case "FraudInvestigation": BankSphere.FraudInvestigation.Api.Extensions.DependencyInjection.AddBankingServices(builder); map = BankSphere.FraudInvestigation.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints; break;
            case "Compliance": BankSphere.Compliance.Api.Extensions.DependencyInjection.AddBankingServices(builder); map = BankSphere.Compliance.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints; break;
            case "CreditAssessment": BankSphere.CreditAssessment.Api.Extensions.DependencyInjection.AddBankingServices(builder); map = BankSphere.CreditAssessment.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints; break;
            default: throw new ArgumentException("Unknown context.", nameof(context));
        }
        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
            options => options.BackchannelHttpHandler = App.GetTestServer().CreateHandler());
        builder.Services.AddHttpClient(BankingSecurity.SessionClient).ConfigurePrimaryHttpMessageHandler(() =>
            sessionUnavailable ? new UnavailableIdentityHandler() : App.GetTestServer().CreateHandler());
        var app = builder.Build(); app.UseRouting(); app.UseBankingApi(); map(app); await app.StartAsync(); return app;
    }

    public DbContextOptions<T> Options<T>(string schema) where T : DbContext => new DbContextOptionsBuilder<T>()
        .UseSqlServer(Connection, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema)).Options;

    private static X509Certificate2 Certificate(string name)
    {
        using var key = RSA.Create(3072);
        var request = new CertificateRequest("CN=" + name, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(1));
    }

    private async Task SeedBankingAsync()
    {
        await using var customers = new BankSphere.Customers.Infrastructure.CustomersDbContext(Options<BankSphere.Customers.Infrastructure.CustomersDbContext>("customers"));
        await using var accounts = new BankSphere.Accounts.Infrastructure.AccountsDbContext(Options<BankSphere.Accounts.Infrastructure.AccountsDbContext>("accounts"));
        await using var transactions = new BankSphere.Transactions.Infrastructure.TransactionsDbContext(Options<BankSphere.Transactions.Infrastructure.TransactionsDbContext>("transactions"));
        await using var fraud = new BankSphere.FraudInvestigation.Infrastructure.FraudInvestigationDbContext(Options<BankSphere.FraudInvestigation.Infrastructure.FraudInvestigationDbContext>("fraudinvestigation"));
        await using var compliance = new BankSphere.Compliance.Infrastructure.ComplianceDbContext(Options<BankSphere.Compliance.Infrastructure.ComplianceDbContext>("compliance"));
        await using var credit = new BankSphere.CreditAssessment.Infrastructure.CreditAssessmentDbContext(Options<BankSphere.CreditAssessment.Infrastructure.CreditAssessmentDbContext>("credit"));
        foreach (var context in new DbContext[] { customers, accounts, transactions, fraud, compliance, credit }) await context.Database.MigrateAsync();
        var now = DateTimeOffset.UtcNow;
        var customer = BankSphere.Customers.Domain.Customer.RegisterCustomer(BankSphere.Customers.Domain.CustomerType.Individual,
            BankSphere.Customers.Domain.CustomerSegment.Retail, new("Synthetic", "Identity", null, new(1990, 1, 1), "Paris", "FR", "FR"), null,
            new("Tester", BankSphere.Customers.Domain.EmploymentStatus.Employed, "Synthetic", null, "Test", "Salary", "Savings", "Salary"), "Test", "en", "email", now);
        await new BankSphere.Customers.Infrastructure.CustomerRepository(customers).AddAsync(customer, default);
        var account = BankSphere.Accounts.Domain.BankAccount.OpenAccount(customer.Id, BankSphere.Accounts.Domain.AccountType.Current, "TEST", "Synthetic", new("EUR"), "TEST", "Testing", new(100, new("EUR")), new(1000, new("EUR")), "test-operator", now);
        await new BankSphere.Accounts.Infrastructure.AccountRepository(accounts).AddAsync(account, default);
        var investigation = BankSphere.FraudInvestigation.Domain.FraudCase.OpenInvestigation(customer.Id, account.Id, Guid.NewGuid(), "Card", "TEST", BankSphere.FraudInvestigation.Domain.FraudSeverity.Low, 1, "simulator", now, 1, "Low", "Synthetic test", new(1, new("EUR")), new(1, new("EUR")), "test-operator", now.AddDays(1), now);
        await new BankSphere.FraudInvestigation.Infrastructure.FraudCaseRepository(fraud).AddAsync(investigation, default);
        var review = BankSphere.Compliance.Domain.ComplianceCase.Open(customer.Id, BankSphere.Compliance.Domain.ComplianceCaseType.Onboarding, "FR", "Test", "Test", now.AddDays(1), now);
        await new BankSphere.Compliance.Infrastructure.ComplianceRepository(compliance).AddAsync(review, default);
        var application = BankSphere.CreditAssessment.Domain.CreditApplication.CreateDraftApplication(customer.Id, "TEST", "Testing", new(1000, new("EUR")), 12, "Test",
            new("Employed", new(3000, new("EUR")), new(500, new("EUR")), new(100, new("EUR")), new(500, new("EUR"))), now);
        await new BankSphere.CreditAssessment.Infrastructure.CreditApplicationRepository(credit).AddAsync(application, default);
        CustomerRoute = $"/customers/{customer.Id}/eligibility"; AccountRoute = $"/accounts/{account.Id}";
        TransactionsRoute = $"/accounts/{account.Id}/transactions"; FraudRoute = $"/investigations/{investigation.Id}";
        ComplianceRoute = $"/compliance-cases/{review.Id}"; CreditRoute = $"/credit-applications/{application.Id}/status";
    }

    public async Task DisposeAsync()
    {
        if (App is not null) await App.DisposeAsync();
        var target = new SqlConnectionStringBuilder(Connection).InitialCatalog;
        if (target != databaseName || !target.StartsWith("BankSphereAI_IdentityTests_", StringComparison.Ordinal)) throw new InvalidOperationException("Test database boundary violation.");
        await using var context = new IdentityDbContext(new DbContextOptionsBuilder<IdentityDbContext>().UseSqlServer(Connection).UseOpenIddict().Options);
        await context.Database.EnsureDeletedAsync();
        signing.Dispose(); encryption.Dispose();
        // Certificate files and Data Protection keys are scoped to a generated test-only directory.
        var expectedRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "BankSphereIdentityTests")) + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(directory).StartsWith(expectedRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Test key directory boundary violation.");
        Directory.Delete(directory, recursive: true);
    }
}
