using BankSphere.FraudInvestigation.Application;
using BankSphere.Security;

namespace BankSphere.FraudInvestigation.Api.Endpoints;

public static class AttachFraudEvidenceEndpoint
{
    public static void MapAttachFraudEvidence(this RouteGroupBuilder group)
    {
        group.MapPost("/investigations/{id:guid}/evidence", async (Guid id, AttachFraudEvidence command, FraudEvidenceHandler handler, HttpContext http, CancellationToken ct) => Results.Ok(await handler.Handle(id, command, http.User.BankingActor(), ct))).RequireAuthorization("banking-operator");
    }
}
