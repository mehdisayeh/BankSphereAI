using BankSphere.Transactions.Application;

namespace BankSphere.Transactions.Api.Endpoints;

public static class GetTransactionsByAccountEndpoint
{
    public static void MapGetTransactionsByAccount(this RouteGroupBuilder group)
    {
        group.MapGet("/accounts/{accountId:guid}/transactions", async (Guid accountId, int? pageNumber, int? pageSize, string? sort, bool? descending,
            BankSphere.Transactions.Domain.TransactionStatus? status, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, GetTransactionsByAccountHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new(accountId, new(pageNumber ?? 1, pageSize ?? 25, sort ?? "date", descending ?? true), status, fromUtc, toUtc), ct))).RequireAuthorization("banking-reader");
    }
}
