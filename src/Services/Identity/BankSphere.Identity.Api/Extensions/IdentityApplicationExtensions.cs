using BankSphere.Identity.Infrastructure;
using BankSphere.ServiceDefaults;

namespace BankSphere.Identity.Api.Extensions;

public static class IdentityApplicationExtensions
{
    public static WebApplication UseIdentityApi(this WebApplication app)
    {
        app.UseForwardedHeaders();
        if (app.Environment.IsProduction()) app.UseHsts();
        app.Use(async (http, next) =>
        {
            if (!http.Request.IsHttps && !http.Request.Path.StartsWithSegments("/health"))
            {
                await Results.Problem(statusCode: 400, title: "identity.https_required").ExecuteAsync(http); return;
            }
            await next(http);
        });
        app.UseRouting();
        app.UseRateLimiter();
        app.UseMiddleware<TokenRedemptionLockMiddleware>();
        app.UseBankingApi();
        return app;
    }
}
