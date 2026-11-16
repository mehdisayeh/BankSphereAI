using BankSphere.Accounts.Application;

namespace BankSphere.Accounts.Api.Endpoints;

public static class GetAccountByIdEndpoint
{
    public static void MapGetAccountById(this RouteGroupBuilder group)
    {
        group.MapGet("/accounts/{id:guid}", async (Guid id, GetAccountByIdHandler handler, CancellationToken ct) => Results.Ok(await handler.Handle(id, ct))).RequireAuthorization("banking-reader");
    }
}
