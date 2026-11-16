using BankSphere.Compliance.Application;
using BankSphere.Security;

namespace BankSphere.Compliance.Api.Endpoints;

public static class OpenComplianceCaseEndpoint
{
    public static void MapOpenComplianceCase(this RouteGroupBuilder group)
    {
        group.MapPost("/compliance-cases", async (OpenComplianceCase command, OpenComplianceCaseHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var value = await handler.Handle(command, http.User.BankingActor(), ct); return Results.Created($"/compliance-cases/{value.Id}", value);
        }).RequireAuthorization("compliance-officer");
    }
}
