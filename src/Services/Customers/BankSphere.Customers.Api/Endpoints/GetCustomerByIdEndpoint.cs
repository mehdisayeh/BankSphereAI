using BankSphere.Customers.Application;

namespace BankSphere.Customers.Api.Endpoints;

public static class GetCustomerByIdEndpoint
{
    public static void MapGetCustomerById(this RouteGroupBuilder group)
    {
        group.MapGet("/customers/{id:guid}", async (Guid id, GetCustomerByIdHandler handler, CancellationToken ct) => Results.Ok(await handler.Handle(id, ct))).RequireAuthorization("banking-reader");
    }
}
