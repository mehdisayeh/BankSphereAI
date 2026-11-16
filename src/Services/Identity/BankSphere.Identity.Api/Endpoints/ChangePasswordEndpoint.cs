using BankSphere.Identity.Application;
using BankSphere.Identity.Api.Contracts.Requests;
using BankSphere.Identity.Api.Extensions;

namespace BankSphere.Identity.Api.Endpoints;

public static class ChangePasswordEndpoint
{
    public static void MapChangePassword(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/change-password", async (ChangePasswordRequest request, ChangePasswordCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            await handler.Handle(new(IdentityEndpointHelpers.UserId(http), request.CurrentPassword, request.NewPassword), ct);
            return Results.NoContent();
        }).RequireAuthorization().RequireRateLimiting("identity-auth");
    }
}
