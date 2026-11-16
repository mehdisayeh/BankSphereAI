using BankSphere.Identity.Application;

namespace BankSphere.Identity.Api.Endpoints;

public static class ForgotPasswordEndpoint
{
    public static void MapForgotPassword(this RouteGroupBuilder group)
    {
        group.MapPost("/identity/forgot-password", async (ForgotPasswordCommand command, ForgotPasswordCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(command, ct); return Results.Accepted();
        }).RequireRateLimiting("identity-auth");
    }
}
