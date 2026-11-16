using BankSphere.Customers.Application;

namespace BankSphere.Customers.Api.Endpoints;

public static class UpdateCustomerContactEndpoint
{
    public static void MapUpdateCustomerContact(this RouteGroupBuilder group)
    {
        group.MapPut("/customers/{id:guid}/contact", async (Guid id, UpdateCustomerContact command, UpdateCustomerContactHandler handler, CancellationToken ct) => Results.Ok(await handler.Handle(id, command, ct))).RequireAuthorization("banking-operator");
    }
}
