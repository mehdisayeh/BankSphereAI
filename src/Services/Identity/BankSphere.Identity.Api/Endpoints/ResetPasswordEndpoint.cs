using BankSphere.Identity.Application;

namespace BankSphere.Identity.Api.Endpoints;

public static class ResetPasswordEndpoint
{
    public static void MapResetPassword(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/reset-password", async (ResetPasswordCommand command, ResetPasswordCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(command, ct); return Results.NoContent();
        }).RequireRateLimiting("identity-auth");
    }
}
