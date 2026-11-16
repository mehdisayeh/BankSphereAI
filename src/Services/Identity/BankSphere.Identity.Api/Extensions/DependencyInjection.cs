using System.Threading.RateLimiting;
using BankSphere.Identity.Application;
using BankSphere.Identity.Infrastructure;
using BankSphere.ServiceDefaults;

namespace BankSphere.Identity.Api.Extensions;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddIdentityServices(this WebApplicationBuilder builder)
    {
        builder.AddBankingApi();
        builder.Services.AddIdentityInfrastructure(builder.Configuration, builder.Environment);
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            foreach (var proxy in builder.Configuration.GetSection("Identity:TrustedProxies").GetChildren())
            {
                if (!System.Net.IPAddress.TryParse(proxy.Value, out var address)) throw new InvalidOperationException("An identity trusted proxy must be an IP address.");
                options.KnownProxies.Add(address);
            }
        });
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 32 * 1024);
        builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.ValueCountLimit = 32; options.ValueLengthLimit = 16 * 1024; options.KeyLengthLimit = 128;
        });
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-BankSphere.Csrf";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        var permitLimit = builder.Configuration.GetValue<int?>("Identity:RateLimit:PermitLimit") ?? 30;
        if (permitLimit is < 1 or > 300) throw new InvalidOperationException("Identity rate limit must be 1..300 requests per minute.");
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("identity-auth", http => RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                { PermitLimit = permitLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
        });
        builder.Services.AddHealthChecks().AddCheck<IdentityDatabaseHealthCheck>("identity-sql");
        builder.Services.AddScoped<AuthorizeCommandHandler>();
        builder.Services.AddScoped<GrantRoleCommandHandler>();
        builder.Services.AddScoped<ChangePasswordCommandHandler>();
        builder.Services.AddScoped<ConfirmEmailCommandHandler>();
        builder.Services.AddScoped<ExchangeAuthorizationCodeCommandHandler>();
        builder.Services.AddScoped<ForgotPasswordCommandHandler>();
        builder.Services.AddScoped<GetCurrentUserQueryHandler>();
        builder.Services.AddScoped<LoginCommandHandler>();
        builder.Services.AddScoped<LogoutCommandHandler>();
        builder.Services.AddScoped<RefreshTokenCommandHandler>();
        builder.Services.AddScoped<RegisterCommandHandler>();
        builder.Services.AddScoped<ResetPasswordCommandHandler>();
        builder.Services.AddScoped<RevokeSessionsCommandHandler>();
        return builder;
    }
}
