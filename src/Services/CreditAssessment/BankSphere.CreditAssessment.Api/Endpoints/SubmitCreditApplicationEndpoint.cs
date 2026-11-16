using BankSphere.CreditAssessment.Application;
using BankSphere.Security;

namespace BankSphere.CreditAssessment.Api.Endpoints;

public static class SubmitCreditApplicationEndpoint
{
    public static void MapSubmitCreditApplication(this RouteGroupBuilder group)
    {
        group.MapPost("/credit-applications", async (SubmitCreditApplication command, SubmitCreditApplicationHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var value = await handler.Handle(command, http.User.BankingActor(), ct); return Results.Created($"/credit-applications/{value.Id}/status", value);
        }).RequireAuthorization("banking-operator");
    }
}
