using BankSphere.CreditAssessment.Application;

namespace BankSphere.CreditAssessment.Api.Endpoints;

public static class GetCreditApplicationStatusEndpoint
{
    public static void MapGetCreditApplicationStatus(this RouteGroupBuilder group)
    {
        group.MapGet("/credit-applications/{id:guid}/status", async (Guid id, GetCreditApplicationStatusHandler handler, CancellationToken ct) => Results.Ok(await handler.Handle(id, ct))).RequireAuthorization("banking-reader");
    }
}
