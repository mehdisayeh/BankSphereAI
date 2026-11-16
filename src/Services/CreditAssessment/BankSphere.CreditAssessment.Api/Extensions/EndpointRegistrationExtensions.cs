using BankSphere.CreditAssessment.Api.Endpoints;

namespace BankSphere.CreditAssessment.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static void MapBankingEndpoints(this WebApplication app)
    {
        // Keep full route templates: prefixed groups append a slash to collection routes.
        var creditapplicationsGroup = app.MapGroup("");
        creditapplicationsGroup.MapSubmitCreditApplication();
        creditapplicationsGroup.MapGetCreditApplicationStatus();
    }
}
