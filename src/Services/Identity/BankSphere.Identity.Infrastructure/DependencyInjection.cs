using System.Security.Cryptography.X509Certificates;
using BankSphere.Identity.Application;
using BankSphere.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BankSphere.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connection = configuration.GetConnectionString("Banking")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Banking.");
        services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(connection,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")).UseOpenIddict());
        services.AddIdentity<ApplicationUser, Role>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 14; options.Password.RequiredUniqueChars = 5;
            options.Password.RequireDigit = true; options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true; options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5; options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.SignIn.RequireConfirmedEmail = true;
        }).AddEntityFrameworkStores<IdentityDbContext>().AddDefaultTokenProviders();
        services.Configure<PasswordHasherOptions>(options => options.IterationCount = 220_000);
        services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1));
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "__Host-BankSphere.Identity"; options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10); options.SlidingExpiration = false;
            options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
            options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
        });
        // Identity cookies are used only at the login/authorization endpoints. APIs use JWTs explicitly.
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events.OnTokenValidated = async context =>
            {
                if (!await context.HttpContext.RequestServices.GetRequiredService<SessionValidator>()
                    .ValidateAsync(context.Principal!, cancellationToken: context.HttpContext.RequestAborted))
                    context.Fail("Session expired or revoked.");
            };
        });
        var protection = services.AddDataProtection().SetApplicationName("BankSphere.Identity");
        var keyDirectory = configuration["Identity:DataProtection:Directory"];
        if (environment.IsProduction() && string.IsNullOrWhiteSpace(keyDirectory))
            throw new InvalidOperationException("Configure a persistent protected Identity:DataProtection:Directory.");
        protection.PersistKeysToFileSystem(new DirectoryInfo(keyDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BankSphereAI", "IdentityKeys")));
        var signing = LoadCertificate(configuration, "Signing", environment);
        var encryption = LoadCertificate(configuration, "Encryption", environment);
        if (signing is not null && encryption is not null && signing.Thumbprint == encryption.Thumbprint)
            throw new InvalidOperationException("Use separate signing and encryption certificates.");
        if (environment.IsProduction())
        {
            foreach (var key in new[] { "Host", "From", "AccountActionUri" })
                if (string.IsNullOrWhiteSpace(configuration["Identity:Notifications:" + key]))
                    throw new InvalidOperationException($"Configure Identity:Notifications:{key} before starting production Identity.");
        }
        if (encryption is not null) protection.ProtectKeysWithCertificate(encryption);
        services.AddOpenIddict().AddCore(options => options.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>())
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(configuration["Authentication:Authority"] ?? BankingSecurity.DefaultAuthority));
                options.SetAuthorizationEndpointUris("connect/authorize").SetTokenEndpointUris("connect/token");
                options.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow().RequireProofKeyForCodeExchange();
                options.Configure(server => server.CodeChallengeMethods.Remove(CodeChallengeMethods.Plain));
                options.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.OfflineAccess, BankingPermissions.Scope);
                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(5)).SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(2))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(30)).SetRefreshTokenReuseLeeway(TimeSpan.Zero);
                options.DisableAccessTokenEncryption();
                if (signing is null) options.AddDevelopmentSigningCertificate(); else options.AddSigningCertificate(signing);
                if (encryption is null) options.AddDevelopmentEncryptionCertificate(); else options.AddEncryptionCertificate(encryption);
                options.UseAspNetCore().EnableAuthorizationEndpointPassthrough().EnableTokenEndpointPassthrough();
                options.AddEventHandler<OpenIddictServerEvents.ValidateTokenContext>(handler => handler.UseScopedHandler<TokenReuseHandler>()
                    .SetOrder(OpenIddictServerHandlers.Protection.ValidateTokenEntry.Descriptor.Order - 500));
                options.AddEventHandler<OpenIddictServerEvents.ApplyTokenResponseContext>(handler => handler.UseScopedHandler<TokenAuditHandler>());
            });
        services.AddScoped<IdentityService>(); services.AddScoped<IIdentityService>(provider => provider.GetRequiredService<IdentityService>());
        services.AddScoped<IIdentityProtocol, IdentityProtocol>(); services.AddScoped<SessionValidator>(); services.AddScoped<SecurityAuditWriter>();
        services.AddScoped<IRoleProvisioningService, RoleProvisioningService>();
        services.AddScoped<IIdentityNotificationSender, SmtpIdentityNotificationSender>();
        services.AddHostedService<IdentityInitializer>();
        return services;
    }

    private static X509Certificate2? LoadCertificate(IConfiguration configuration, string purpose, IHostEnvironment environment)
    {
        var path = configuration[$"Identity:{purpose}Certificate:Path"];
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!environment.IsDevelopment()) throw new InvalidOperationException($"Configure Identity:{purpose}Certificate:Path with a private RSA certificate.");
            return null;
        }
        var certificate = X509CertificateLoader.LoadPkcs12FromFile(path, configuration[$"Identity:{purpose}Certificate:Password"], X509KeyStorageFlags.EphemeralKeySet);
        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa is null || rsa.KeySize < 3072 || certificate.NotAfter.ToUniversalTime() <= DateTime.UtcNow || certificate.NotBefore.ToUniversalTime() > DateTime.UtcNow)
            throw new InvalidOperationException($"The {purpose} certificate requires a valid RSA private key of at least 3072 bits.");
        return certificate;
    }
}
