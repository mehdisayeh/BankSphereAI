using BankSphere.Customers.Application;

namespace BankSphere.Customers.Api.Endpoints;

public static class GetCustomerEligibilityEndpoint
{
    public static void MapGetCustomerEligibility(this RouteGroupBuilder group)
    {
        group.MapGet("/customers/{id:guid}/eligibility", async (Guid id, GetCustomerEligibilityHandler handler, CancellationToken ct) =>
            await handler.Handle(id, ct) is { } value ? Results.Ok(value) : Results.NotFound()).RequireAuthorization("banking-reader");
    }
}
