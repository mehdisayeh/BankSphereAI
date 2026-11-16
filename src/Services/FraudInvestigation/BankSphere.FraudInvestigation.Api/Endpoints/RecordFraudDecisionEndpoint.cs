using BankSphere.FraudInvestigation.Application;
using BankSphere.Security;

namespace BankSphere.FraudInvestigation.Api.Endpoints;

public static class RecordFraudDecisionEndpoint
{
    public static void MapRecordFraudDecision(this RouteGroupBuilder group)
    {
        group.MapPost("/investigations/{id:guid}/decisions", async (Guid id, RecordFraudDecision command, RecordFraudDecisionHandler handler, HttpContext http, CancellationToken ct) => Results.Ok(await handler.Handle(id, command, http.User.BankingActor(), ct))).RequireAuthorization("fraud-reviewer");
    }
}
