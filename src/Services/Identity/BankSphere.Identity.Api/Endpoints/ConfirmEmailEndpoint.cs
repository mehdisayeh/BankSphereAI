using BankSphere.Identity.Application;

namespace BankSphere.Identity.Api.Endpoints;

public static class ConfirmEmailEndpoint
{
    public static void MapConfirmEmail(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/confirm-email", async (ConfirmEmailCommand command, ConfirmEmailCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(command, ct); return Results.NoContent();
        }).RequireRateLimiting("identity-auth");
    }
}
