using BankSphere.Compliance.Api.Endpoints;

namespace BankSphere.Compliance.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static void MapBankingEndpoints(this WebApplication app)
    {
        // Keep full route templates: prefixed groups append a slash to collection routes.
        var compliancecasesGroup = app.MapGroup("");
        compliancecasesGroup.MapOpenComplianceCase();
        compliancecasesGroup.MapGetComplianceCase();
    }
}
