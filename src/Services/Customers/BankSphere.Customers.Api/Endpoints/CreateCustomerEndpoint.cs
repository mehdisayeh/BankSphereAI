using BankSphere.Customers.Application;

namespace BankSphere.Customers.Api.Endpoints;

public static class CreateCustomerEndpoint
{
    public static void MapCreateCustomer(this RouteGroupBuilder group)
    {
        group.MapPost("/customers", async (CreateCustomer command, CreateCustomerHandler handler, CancellationToken ct) =>
        {
            var value = await handler.Handle(command, ct); return Results.Created($"/customers/{value.Id}", value);
        }).RequireAuthorization("banking-operator");
    }
}
