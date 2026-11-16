using BankSphere.Accounts.Api.Endpoints;

namespace BankSphere.Accounts.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static void MapBankingEndpoints(this WebApplication app)
    {
        // Keep full route templates: prefixed groups append a slash to collection routes.
        var accountsGroup = app.MapGroup("");
        accountsGroup.MapOpenAccount();
        accountsGroup.MapGetAccountById();
    }
}
