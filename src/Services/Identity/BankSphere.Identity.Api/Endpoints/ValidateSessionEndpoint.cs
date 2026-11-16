
namespace BankSphere.Identity.Api.Endpoints;

public static class ValidateSessionEndpoint
{
    public static void MapValidateSession(this RouteGroupBuilder group)
    {
        group.MapGet("/identity/sessions/validate", (HttpContext http) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
