using BankSphere.Identity.Application;
using BankSphere.Identity.Api.Extensions;
using Microsoft.AspNetCore.Antiforgery;

namespace BankSphere.Identity.Api.Endpoints;

public static class RegisterEndpoint
{
    public static void MapRegister(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/register", async (RegisterCommand command, RegisterCommandHandler handler, HttpContext http, IAntiforgery antiforgery, CancellationToken ct) =>
        {
            await IdentityEndpointHelpers.ValidateCsrfAsync(http, antiforgery);
            await handler.Handle(command, ct); return Results.Accepted();
        }).RequireRateLimiting("identity-auth");
    }
}
