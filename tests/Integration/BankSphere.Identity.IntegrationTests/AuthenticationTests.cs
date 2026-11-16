using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using BankSphere.Identity.Infrastructure;
using BankSphere.Identity.Application;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using BankSphere.Persistence.Tests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BankSphere.Identity.IntegrationTests;

public sealed class AuthenticationTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    private static readonly string[] EscalatedRoles = ["banking-operator"];

    [Fact]
    public async Task PkceVerifierIsRequiredAndCodeReplayRevokesTheSession()
    {
        var (user, password) = await fixture.UserAsync("banking-reader");
        using var browser = fixture.Browser(); await IdentityFixture.AddCsrfAsync(browser);
        using var login = await browser.PostAsJsonAsync("/identity/login", new { user.Email, password }); Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var (firstCode, _) = await IdentityFixture.AuthorizeAsync(browser);
        using var wrong = await browser.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code", ["client_id"] = "integration-client", ["redirect_uri"] = "https://client.test/callback",
            ["code"] = firstCode, ["code_verifier"] = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32))
        }));
        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        var (code, verifier) = await IdentityFixture.AuthorizeAsync(browser);
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code", ["client_id"] = "integration-client", ["redirect_uri"] = "https://client.test/callback",
            ["code"] = code, ["code_verifier"] = verifier
        };
        using var first = await browser.PostAsync("/connect/token", new FormUrlEncodedContent(parameters)); first.EnsureSuccessStatusCode();
        var token = await first.Content.ReadFromJsonAsync<JsonElement>();
        using var replay = await browser.PostAsync("/connect/token", new FormUrlEncodedContent(parameters)); Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        using var client = fixture.Client(token.GetProperty("access_token").GetString());
        using var denied = await client.GetAsync("/identity/me"); Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
    }

    [Fact]
    public async Task RefreshScopeCanBeReducedButNotExpanded()
    {
        var token = await fixture.TokensAsync("banking-reader");
        using var client = fixture.Client();
        using var reduced = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token", ["client_id"] = "integration-client",
            ["refresh_token"] = token.GetProperty("refresh_token").GetString()!, ["scope"] = "openid offline_access"
        }));
        var json = await reduced.Content.ReadFromJsonAsync<JsonElement>(); Assert.True(reduced.IsSuccessStatusCode, json.ToString());
        await using var bank = await fixture.BankingAppAsync("Customers"); using var api = bank.GetTestClient(); api.BaseAddress = new Uri("https://banking.test/");
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.GetProperty("access_token").GetString());
        using var denied = await api.GetAsync(fixture.CustomerRoute); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var expanded = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token", ["client_id"] = "integration-client",
            ["refresh_token"] = json.GetProperty("refresh_token").GetString()!, ["scope"] = "openid banking offline_access"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, expanded.StatusCode);
    }

    [Fact]
    public async Task PasswordGrantAndPlainHttpAreRejected()
    {
        using var client = fixture.Client();
        using var unsupported = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["grant_type"] = "password", ["client_id"] = "integration-client", ["username"] = "synthetic@example.invalid", ["password"] = IdentityFixture.Password() }));
        Assert.Equal(HttpStatusCode.BadRequest, unsupported.StatusCode);
        using var insecure = await client.GetAsync("http://identity.test/identity/antiforgery"); Assert.Equal(HttpStatusCode.BadRequest, insecure.StatusCode);
    }

    [Fact]
    public async Task ConcurrentRefreshAllowsOnlyOneRedemption()
    {
        var token = await fixture.TokensAsync("banking-reader");
        var refresh = token.GetProperty("refresh_token").GetString()!;
        var responses = await Task.WhenAll(fixture.RefreshAsync(refresh), fixture.RefreshAsync(refresh));
        try
        {
            Assert.Single(responses, response => response.IsSuccessStatusCode);
            Assert.Single(responses, response => response.StatusCode == HttpStatusCode.BadRequest);
        }
        finally { foreach (var response in responses) response.Dispose(); }
    }

    [Fact]
    public async Task DeploymentProvisioningUsesOnlyExistingRolesAndHasNoHttpRoute()
    {
        var (user, _) = await fixture.UserAsync();
        using var scope = fixture.App.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GrantRoleCommandHandler>();
        await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new(user.Email!, "administrator"), default));
        await handler.Handle(new(user.Email!, "banking-reader"), default);
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.True(await users.IsInRoleAsync((await users.FindByIdAsync(user.Id.ToString()))!, "banking-reader"));
        using var client = fixture.Client();
        using var route = await client.PostAsJsonAsync("/identity/grant-role", new { email = user.Email, role = "banking-operator" });
        Assert.Equal(HttpStatusCode.NotFound, route.StatusCode);
    }

    [Fact]
    public async Task PermissionRemovalIsEffectiveImmediatelyAndIdentityOutagesFailClosed()
    {
        var token = await fixture.TokensAsync("banking-reader");
        var access = token.GetProperty("access_token").GetString()!;
        await using (var unavailable = await fixture.BankingAppAsync("Customers", sessionUnavailable: true))
        {
            using var client = unavailable.GetTestClient(); client.BaseAddress = new Uri("https://banking.test/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access);
            using var response = await client.GetAsync(fixture.CustomerRoute); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        using var scope = fixture.App.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var grant = await db.RolePermissions.SingleAsync(value => value.PermissionName == "banking-reader");
        db.RolePermissions.Remove(grant); await db.SaveChangesAsync();
        try
        {
            await using var bank = await fixture.BankingAppAsync("Customers"); using var client = bank.GetTestClient();
            client.BaseAddress = new Uri("https://banking.test/"); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access);
            using var response = await client.GetAsync(fixture.CustomerRoute); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        finally { db.RolePermissions.Add(grant); await db.SaveChangesAsync(); }
    }

    [Fact]
    public void ProductionCannotStartWithDevelopmentSigningKeys()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Banking"] = fixture.Connection,
            ["Identity:DataProtection:Directory"] = Path.GetTempPath()
        });
        var error = Assert.Throws<InvalidOperationException>(() => BankSphere.Identity.Api.Extensions.DependencyInjection.AddIdentityServices(builder));
        Assert.Contains("SigningCertificate", error.Message, StringComparison.Ordinal);
    }
    [Fact]
    public async Task IdentitySchemaAndHistoryMatchTheModel()
    {
        using var scope = fixture.App.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var result = await SqlSchemaVerifier.VerifyAsync(database, "identity");
        Assert.NotEmpty(result.Tables);
        Assert.Single(result.AppliedMigrations);
    }

    [Fact]
    public async Task PublicRegistrationCannotAssignRolesAndRequiresEmailConfirmation()
    {
        var email = Guid.NewGuid().ToString("N") + "@example.invalid"; var password = IdentityFixture.Password();
        using var browser = fixture.Browser(); await IdentityFixture.AddCsrfAsync(browser);
        using var registration = await browser.PostAsJsonAsync("/identity/register", new
            { email, password, displayName = "Synthetic", roles = EscalatedRoles, customerId = Guid.NewGuid(), emailConfirmed = true });
        Assert.Equal(HttpStatusCode.Accepted, registration.StatusCode);
        using var denied = await browser.PostAsJsonAsync("/identity/login", new { email, password });
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var scope = fixture.App.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByEmailAsync(email))!;
        Assert.False(user.EmailConfirmed); Assert.Null(user.CustomerId); Assert.Empty(await users.GetRolesAsync(user));
        Assert.NotEqual(password, user.PasswordHash);
        Assert.NotEqual(PasswordVerificationResult.Failed, users.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash!, password));
        using var confirmation = await browser.PostAsJsonAsync("/identity/confirm-email", new { email, token = fixture.Notifications.Token(email, "confirm-email") });
        Assert.Equal(HttpStatusCode.NoContent, confirmation.StatusCode);
        using var login = await browser.PostAsJsonAsync("/identity/login", new { email, password });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
    }

    [Fact]
    public async Task LoginRequiresCsrfAndWrongPasswordsLockTheAccount()
    {
        var (user, password) = await fixture.UserAsync(); using var browser = fixture.Browser();
        using var noCsrf = await browser.PostAsJsonAsync("/identity/login", new { user.Email, password });
        Assert.Equal(HttpStatusCode.BadRequest, noCsrf.StatusCode);
        await IdentityFixture.AddCsrfAsync(browser);
        for (var i = 0; i < 5; i++)
        {
            using var wrong = await browser.PostAsJsonAsync("/identity/login", new { user.Email, password = IdentityFixture.Password() });
            Assert.Equal(HttpStatusCode.Forbidden, wrong.StatusCode);
        }
        using var locked = await browser.PostAsJsonAsync("/identity/login", new { user.Email, password });
        Assert.Equal(HttpStatusCode.Forbidden, locked.StatusCode);
        using var scope = fixture.App.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.True(await users.IsLockedOutAsync((await users.FindByIdAsync(user.Id.ToString()))!));
    }

    [Theory]
    [InlineData("Customers")]
    [InlineData("Accounts")]
    [InlineData("Transactions")]
    [InlineData("FraudInvestigation")]
    [InlineData("Compliance")]
    [InlineData("CreditAssessment")]
    public async Task EveryBankingApiValidatesIdentityJwtAndEnforcesItsPolicy(string context)
    {
        var role = context == "Compliance" ? "compliance-officer" : "banking-reader";
        var token = (await fixture.TokensAsync(role)).GetProperty("access_token").GetString()!;
        await using var app = await fixture.BankingAppAsync(context); using var client = app.GetTestClient();
        client.BaseAddress = new Uri("https://banking.test/");
        var route = context switch
        {
            "Customers" => fixture.CustomerRoute, "Accounts" => fixture.AccountRoute, "Transactions" => fixture.TransactionsRoute,
            "FraudInvestigation" => fixture.FraudRoute, "Compliance" => fixture.ComplianceRoute, _ => fixture.CreditRoute
        };
        using var anonymous = await client.GetAsync(route); Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var authorized = await client.GetAsync(route);
        Assert.True(authorized.StatusCode == HttpStatusCode.OK, $"{context}: {authorized.StatusCode} {await authorized.Content.ReadAsStringAsync()}");
        var unprivileged = (await fixture.TokensAsync()).GetProperty("access_token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", unprivileged);
        using var forbidden = await client.GetAsync(route); Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^8] + "tampered");
        using var tampered = await client.GetAsync(route); Assert.Equal(HttpStatusCode.Unauthorized, tampered.StatusCode);
    }

    [Fact]
    public async Task RefreshRotatesAndReplayRevokesTheEntireSessionIncludingBankingAccess()
    {
        var original = await fixture.TokensAsync("banking-reader");
        var refresh = original.GetProperty("refresh_token").GetString()!;
        using var rotatedResponse = await fixture.RefreshAsync(refresh);
        var rotated = await rotatedResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(rotatedResponse.IsSuccessStatusCode, rotated.ToString());
        Assert.NotEqual(refresh, rotated.GetProperty("refresh_token").GetString());
        using (var client = fixture.Client(rotated.GetProperty("access_token").GetString()))
        using (var profile = await client.GetAsync("/identity/me")) Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        using var replay = await fixture.RefreshAsync(refresh); Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        using var family = await fixture.RefreshAsync(rotated.GetProperty("refresh_token").GetString()!);
        Assert.Equal(HttpStatusCode.BadRequest, family.StatusCode);
        await using var bank = await fixture.BankingAppAsync("Customers"); using var api = bank.GetTestClient();
        api.BaseAddress = new Uri("https://banking.test/");
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rotated.GetProperty("access_token").GetString());
        using var rejected = await api.GetAsync(fixture.CustomerRoute); Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
        using var scope = fixture.App.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        Assert.True(await database.SecurityAuditLogs.AnyAsync(log => log.Action == "refresh.reuse-detected"));
    }

    [Fact]
    public async Task LogoutAndSessionRevocationEnforceOwnership()
    {
        var owner = await fixture.TokensAsync(); var other = await fixture.TokensAsync();
        var otherJwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(other.GetProperty("access_token").GetString());
        var otherSession = otherJwt.Claims.Single(claim => claim.Type == "sid").Value;
        using var ownerClient = fixture.Client(owner.GetProperty("access_token").GetString());
        using var forbidden = await ownerClient.PostAsJsonAsync("/identity/revoke-sessions", new { sessionId = otherSession });
        Assert.Equal(HttpStatusCode.NotFound, forbidden.StatusCode);
        using (var otherClient = fixture.Client(other.GetProperty("access_token").GetString()))
        using (var unchanged = await otherClient.GetAsync("/identity/me")) Assert.Equal(HttpStatusCode.OK, unchanged.StatusCode);
        using var logout = await ownerClient.PostAsync("/identity/logout", null); Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        using var denied = await ownerClient.GetAsync("/identity/me"); Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
        using var refresh = await fixture.RefreshAsync(owner.GetProperty("refresh_token").GetString()!); Assert.Equal(HttpStatusCode.BadRequest, refresh.StatusCode);
    }

    [Fact]
    public async Task PasswordResetDoesNotDiscloseTokensAndRevokesSessions()
    {
        var token = await fixture.TokensAsync(); using var client = fixture.Client(token.GetProperty("access_token").GetString());
        var profile = await client.GetFromJsonAsync<JsonElement>("/identity/me"); var email = profile.GetProperty("email").GetString()!;
        using var request = await client.PostAsJsonAsync("/identity/forgot-password", new { email });
        Assert.Equal(HttpStatusCode.Accepted, request.StatusCode); Assert.Equal("", await request.Content.ReadAsStringAsync());
        var reset = fixture.Notifications.Token(email, "reset-password"); var next = IdentityFixture.Password();
        using var resetResponse = await client.PostAsJsonAsync("/identity/reset-password", new { email, token = reset, newPassword = next });
        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);
        using var expired = await client.GetAsync("/identity/me"); Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
        using var replay = await client.PostAsJsonAsync("/identity/reset-password", new { email, token = reset, newPassword = IdentityFixture.Password() });
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        using var browser = fixture.Browser(); await IdentityFixture.AddCsrfAsync(browser);
        using var login = await browser.PostAsJsonAsync("/identity/login", new { email, password = next }); Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
    }

    [Fact]
    public async Task PasswordChangeAndRevokeAllInvalidateAccessAndRefreshTokens()
    {
        var (user, password) = await fixture.UserAsync();
        using var browser = fixture.Browser(); await IdentityFixture.AddCsrfAsync(browser);
        using var login = await browser.PostAsJsonAsync("/identity/login", new { user.Email, password }); Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var (code, verifier) = await IdentityFixture.AuthorizeAsync(browser);
        using var response = await browser.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
            { ["grant_type"] = "authorization_code", ["client_id"] = "integration-client", ["redirect_uri"] = "https://client.test/callback", ["code"] = code, ["code_verifier"] = verifier }));
        response.EnsureSuccessStatusCode(); var token = await response.Content.ReadFromJsonAsync<JsonElement>();
        using var client = fixture.Client(token.GetProperty("access_token").GetString());
        using var changed = await client.PostAsJsonAsync("/identity/change-password", new { currentPassword = password, newPassword = IdentityFixture.Password() });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        using var rejected = await fixture.RefreshAsync(token.GetProperty("refresh_token").GetString()!); Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        var session = await fixture.TokensAsync(); using var all = fixture.Client(session.GetProperty("access_token").GetString());
        using var revoke = await all.PostAsJsonAsync("/identity/revoke-sessions", new { }); Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);
        using var denied = await all.GetAsync("/identity/me"); Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
    }

    [Fact]
    public async Task PkceAndRedirectUriAreRequiredAndRateLimitingIsEnforced()
    {
        using var client = fixture.Client();
        using var noPkce = await client.GetAsync("/connect/authorize?client_id=integration-client&redirect_uri=https%3A%2F%2Fclient.test%2Fcallback&response_type=code&scope=openid");
        Assert.False(noPkce.IsSuccessStatusCode);
        if (noPkce.StatusCode == HttpStatusCode.Redirect) Assert.DoesNotContain("code=", noPkce.Headers.Location!.Query, StringComparison.Ordinal);
        using var redirect = await client.GetAsync("/connect/authorize?client_id=integration-client&redirect_uri=https%3A%2F%2Fattacker.test%2Fcallback&response_type=code&scope=openid");
        Assert.Equal(HttpStatusCode.BadRequest, redirect.StatusCode);
        await using var limited = await fixture.CreateIdentityAppAsync(1); using var browser = limited.GetTestClient(); browser.BaseAddress = new Uri("https://identity.test/");
        using var first = await browser.GetAsync("/identity/antiforgery"); Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        using var second = await browser.GetAsync("/identity/antiforgery"); Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
    }
}
