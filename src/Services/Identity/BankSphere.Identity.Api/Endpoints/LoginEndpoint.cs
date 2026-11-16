using BankSphere.Identity.Application;
using BankSphere.Identity.Infrastructure;
using BankSphere.Identity.Api.Extensions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace BankSphere.Identity.Api.Endpoints;

public static class LoginEndpoint
{
    public static void MapLogin(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/login", async (LoginCommand command, LoginCommandHandler handler, SignInManager<ApplicationUser> signIn,
            UserManager<ApplicationUser> users, HttpContext http, IAntiforgery antiforgery, CancellationToken ct) =>
        {
            await IdentityEndpointHelpers.ValidateCsrfAsync(http, antiforgery);
            var id = await handler.Handle(command, ct);
            await signIn.SignInAsync((await users.FindByIdAsync(id.ToString()))!, isPersistent: false);
            http.Response.Headers.CacheControl = "no-store";
            return Results.NoContent();
        }).RequireRateLimiting("identity-auth");
    }
}
