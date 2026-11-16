using BankSphere.Transactions.Api.Endpoints;

namespace BankSphere.Transactions.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static void MapBankingEndpoints(this WebApplication app)
    {
        // Keep full route templates: prefixed groups append a slash to collection routes.
        var simulationGroup = app.MapGroup("");
        simulationGroup.MapIngestSimulation();
        var transactionsGroup = app.MapGroup("");
        transactionsGroup.MapGetTransactionById();
        var accountsGroup = app.MapGroup("");
        accountsGroup.MapGetTransactionsByAccount();
    }
}
