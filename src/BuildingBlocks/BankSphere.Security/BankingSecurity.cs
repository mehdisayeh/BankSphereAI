using System.Net.Http.Headers;
using System.Security.Claims;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BankSphere.Security;

public static class BankingSecurity
{
    public const string DefaultAuthority = "https://localhost:7100/";
    public const string DefaultAudience = "banksphere";
    public const string SessionClient = "banksphere-identity-sessions";

    public static IServiceCollection AddBankingSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Authentication:Authority"] ?? DefaultAuthority;
        if (!Uri.TryCreate(authority, UriKind.Absolute, out var issuer) || issuer.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Authentication:Authority must be an absolute HTTPS issuer URI.");
        services.AddHttpClient(SessionClient, client => client.Timeout = TimeSpan.FromSeconds(5));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = issuer.AbsoluteUri;
            options.Audience = configuration["Authentication:Audience"] ?? DefaultAudience;
            options.RequireHttpsMetadata = true;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true, ValidIssuer = issuer.AbsoluteUri,
                ValidateAudience = true, ValidAudience = options.Audience,
                ValidateLifetime = true, RequireExpirationTime = true, RequireSignedTokens = true,
                ValidateIssuerSigningKey = true, ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ValidTypes = ["at+jwt"], NameClaimType = "sub", RoleClaimType = "role", ClockSkew = TimeSpan.FromSeconds(30)
            };
            options.Events = new JwtBearerEvents();
            options.Events.OnTokenValidated = async context =>
            {
                // Independently validate JWTs here, then fail closed on revoked sessions or stale grants.
                var client = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient(SessionClient);
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(issuer, "identity/sessions/validate"));
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(context.Request.Headers.Authorization.ToString());
                try
                {
                    using var response = await client.SendAsync(request, context.HttpContext.RequestAborted);
                    if (!response.IsSuccessStatusCode) context.Fail("The identity session is no longer valid.");
                }
                catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
                {
                    context.Fail("Identity session verification is unavailable.");
                }
            };
        });
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        var authorization = services.AddAuthorizationBuilder();
        foreach (var permission in BankingPermissions.All)
        {
            authorization.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().RequireRole(permission)
                .AddRequirements(new PermissionRequirement(permission))
                .RequireAssertion(context => context.User.FindAll("scope").Any(claim => claim.Value.Split(' ').Contains(BankingPermissions.Scope, StringComparer.Ordinal))));
            authorization.AddPolicy("permission:" + permission, policy => policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(permission)));
        }
        return services;
    }

    public static RequestActor BankingActor(this ClaimsPrincipal user) => new(
        user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
        user.FindAll("role").Concat(user.FindAll(ClaimTypes.Role)).Select(claim => claim.Value).ToHashSet(StringComparer.Ordinal));
}
