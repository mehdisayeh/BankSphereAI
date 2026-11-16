using BankSphere.Customers.Api.Endpoints;

namespace BankSphere.Customers.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static void MapBankingEndpoints(this WebApplication app)
    {
        // Keep full route templates: prefixed groups append a slash to collection routes.
        var customersGroup = app.MapGroup("");
        customersGroup.MapCreateCustomer();
        customersGroup.MapGetCustomerById();
        customersGroup.MapUpdateCustomerContact();
        customersGroup.MapGetCustomerEligibility();
    }
}
