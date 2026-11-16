using Microsoft.AspNetCore.Antiforgery;

namespace BankSphere.Identity.Api.Endpoints;

public static class AntiforgeryEndpoint
{
    public static void MapAntiforgery(this RouteGroupBuilder group)
    {
        group.MapGet("/identity/antiforgery", (HttpContext http, IAntiforgery antiforgery) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            var tokens = antiforgery.GetAndStoreTokens(http);
            return Results.Ok(new { tokens.RequestToken, HeaderName = "X-CSRF-TOKEN" });
        }).RequireRateLimiting("identity-auth");
    }
}
