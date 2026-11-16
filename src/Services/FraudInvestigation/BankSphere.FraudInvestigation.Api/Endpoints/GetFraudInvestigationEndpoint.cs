using BankSphere.FraudInvestigation.Application;

namespace BankSphere.FraudInvestigation.Api.Endpoints;

public static class GetFraudInvestigationEndpoint
{
    public static void MapGetFraudInvestigation(this RouteGroupBuilder group)
    {
        group.MapGet("/investigations/{id:guid}", async (Guid id, GetFraudInvestigationHandler handler, CancellationToken ct) => Results.Ok(await handler.Handle(id, ct))).RequireAuthorization("banking-reader");
    }
}
