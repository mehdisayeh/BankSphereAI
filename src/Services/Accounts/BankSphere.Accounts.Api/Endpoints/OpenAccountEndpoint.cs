using BankSphere.Accounts.Application;
using BankSphere.Security;

namespace BankSphere.Accounts.Api.Endpoints;

public static class OpenAccountEndpoint
{
    public static void MapOpenAccount(this RouteGroupBuilder group)
    {
        group.MapPost("/accounts", async (OpenAccount command, OpenAccountHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var value = await handler.Handle(command, http.User.BankingActor(), ct); return Results.Created($"/accounts/{value.Id}", value);
        }).RequireAuthorization("banking-operator");
    }
}
