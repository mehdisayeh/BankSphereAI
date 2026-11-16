using BankSphere.Transactions.Application;
using BankSphere.Security;

namespace BankSphere.Transactions.Api.Endpoints;

public static class IngestSimulationEndpoint
{
    public static void MapIngestSimulation(this RouteGroupBuilder group)
    {
        group.MapPost("/simulation/transactions", async (IngestSimulation command, IngestSimulationHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, http.User.BankingActor(), ct);
            return result.Replayed ? Results.Ok(result.Transaction) : Results.Created($"/transactions/{result.Transaction.Id}", result.Transaction);
        }).RequireAuthorization("simulator");
    }
}
