using BankSphere.Identity.Application;
using BankSphere.Identity.Api.Extensions;

namespace BankSphere.Identity.Api.Endpoints;

public static class GetCurrentUserEndpoint
{
    public static void MapGetCurrentUser(this RouteGroupBuilder group)
    {
        group.MapGet("/identity/me", async (GetCurrentUserQueryHandler handler, HttpContext http, CancellationToken ct) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            return Results.Ok(await handler.Handle(new(IdentityEndpointHelpers.UserId(http)), ct));
        }).RequireAuthorization();
    }
}
