using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using BankSphere.Identity.Application;
using BankSphere.Identity.Domain;
using BankSphere.Security;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace BankSphere.Identity.Tests;

public sealed class SecurityTests
{
    [Fact]
    public void RefreshMetadataHasAbsoluteExpiryAndIdempotentRevocation()
    {
        var now = DateTimeOffset.UtcNow;
        var token = new RefreshToken(Guid.NewGuid().ToString(), Guid.NewGuid(), "client", Guid.NewGuid().ToString(), now);
        Assert.True(token.IsActive(now));
        Assert.False(token.IsActive(now.AddDays(30)));
        token.Revoke(now.AddMinutes(1), "logout"); token.Revoke(now.AddMinutes(2), "again");
        Assert.Equal(now.AddMinutes(1), token.RevokedAtUtc);
        Assert.Equal("logout", token.RevocationReason);
        Assert.Throws<InvalidOperationException>(() => token.Use(now.AddMinutes(3)));
    }

    [Fact]
    public void CommandsValidateAndDoNotPrintCredentials()
    {
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var command = new RegisterCommand("test@example.invalid", secret, "Synthetic user");
        RegisterCommandValidator.Validate(command);
        Assert.DoesNotContain(secret, command.ToString(), StringComparison.Ordinal);
        Assert.Throws<BusinessException>(() => RegisterCommandValidator.Validate(command with { Email = "invalid" }));
        Assert.Throws<BusinessException>(() => ResetPasswordCommandValidator.Validate(new("test@example.invalid", "", secret)));
        Assert.Throws<BusinessException>(() => GetCurrentUserQueryValidator.Validate(new(Guid.Empty)));
    }

    [Theory]
    [InlineData("banking-reader")]
    [InlineData("banking-operator")]
    [InlineData("fraud-reviewer")]
    [InlineData("credit-officer")]
    [InlineData("compliance-officer")]
    [InlineData("simulator")]
    public async Task ExistingPoliciesRequireBothRoleAndPermission(string name)
    {
        var services = new ServiceCollection(); services.AddLogging();
        services.AddBankingSecurity(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        ClaimsPrincipal User(params Claim[] claims) => new(new ClaimsIdentity(claims, "test", "sub", "role"));
        var role = new Claim("role", name); var permission = new Claim("permission", name); var scope = new Claim("scope", "banking");
        Assert.True((await authorization.AuthorizeAsync(User(role, permission, scope), null, name)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(User(role, scope), null, name)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(User(permission, scope), null, name)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(User(role, permission), null, name)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(User(permission), null, "permission:" + name)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity([permission])), null, "permission:" + name)).Succeeded);
    }

    [Theory]
    [InlineData("valid", true)]
    [InlineData("issuer", false)]
    [InlineData("audience", false)]
    [InlineData("expired", false)]
    [InlineData("signature", false)]
    [InlineData("type", false)]
    [InlineData("algorithm", false)]
    public void SharedJwtConfigurationRejectsInvalidTokens(string test, bool valid)
    {
        var services = new ServiceCollection(); services.AddLogging();
        services.AddBankingSecurity(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var parameters = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters.Clone();
        using var rsa = RSA.Create(3072); using var other = RSA.Create(3072);
        parameters.IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false));
        var now = DateTime.UtcNow;
        var token = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
        {
            Issuer = test == "issuer" ? "https://wrong.example.invalid/" : BankingSecurity.DefaultAuthority,
            Audience = test == "audience" ? "wrong" : "banksphere",
            IssuedAt = now.AddHours(-2), NotBefore = now.AddHours(-2), Expires = test == "expired" ? now.AddMinutes(-2) : now.AddMinutes(2),
            TokenType = test == "type" ? "JWT" : "at+jwt",
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(test == "signature" ? other : rsa),
                test == "algorithm" ? SecurityAlgorithms.RsaSha512 : SecurityAlgorithms.RsaSha256)
        });
        if (valid) Assert.NotNull(new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _));
        else Assert.ThrowsAny<SecurityTokenException>(() => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _));
    }
}
