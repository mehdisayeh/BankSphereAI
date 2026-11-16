using BankSphere.Identity.Application;
using BankSphere.Identity.Api.Contracts.Requests;
using BankSphere.Identity.Api.Extensions;

namespace BankSphere.Identity.Api.Endpoints;

public static class RevokeSessionsEndpoint
{
    public static void MapRevokeSessions(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/revoke-sessions", async (RevokeSessionsRequest request, RevokeSessionsCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            await handler.Handle(new(IdentityEndpointHelpers.UserId(http), request.SessionId), ct); return Results.NoContent();
        }).RequireAuthorization().RequireRateLimiting("identity-auth");
    }
}
