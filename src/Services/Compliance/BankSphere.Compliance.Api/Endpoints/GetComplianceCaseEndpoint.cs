using BankSphere.Compliance.Application;

namespace BankSphere.Compliance.Api.Endpoints;

public static class GetComplianceCaseEndpoint
{
    public static void MapGetComplianceCase(this RouteGroupBuilder group)
    {
        group.MapGet("/compliance-cases/{id:guid}", async (Guid id, GetComplianceCaseHandler handler, CancellationToken ct) => Results.Ok(await handler.Handle(id, ct))).RequireAuthorization("compliance-officer");
    }
}
