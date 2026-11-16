using BankSphere.FraudInvestigation.Application;
using BankSphere.Security;

namespace BankSphere.FraudInvestigation.Api.Endpoints;

public static class OpenFraudInvestigationEndpoint
{
    public static void MapOpenFraudInvestigation(this RouteGroupBuilder group)
    {
        group.MapPost("/investigations", async (OpenFraudInvestigation command, OpenFraudInvestigationHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var value = await handler.Handle(command, http.User.BankingActor(), ct); return Results.Created($"/investigations/{value.Id}", value);
        }).RequireAuthorization("banking-operator");
    }
}
