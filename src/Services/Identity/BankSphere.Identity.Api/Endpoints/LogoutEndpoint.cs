using BankSphere.Identity.Application;
using BankSphere.Identity.Api.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace BankSphere.Identity.Api.Endpoints;

public static class LogoutEndpoint
{
    public static void MapLogout(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/logout", async (LogoutCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            await handler.Handle(new(IdentityEndpointHelpers.UserId(http), IdentityEndpointHelpers.SessionId(http)), ct);
            await http.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Results.NoContent();
        }).RequireAuthorization().RequireRateLimiting("identity-auth");
    }
}
