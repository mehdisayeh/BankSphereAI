using BankSphere.Transactions.Application;

namespace BankSphere.Transactions.Api.Endpoints;

public static class GetTransactionByIdEndpoint
{
    public static void MapGetTransactionById(this RouteGroupBuilder group)
    {
        group.MapGet("/transactions/{id:guid}", async (Guid id, GetTransactionByIdHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(id, ct))).RequireAuthorization("banking-reader");
    }
}
