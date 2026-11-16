using BankSphere.FraudInvestigation.Api.Endpoints;

namespace BankSphere.FraudInvestigation.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static void MapBankingEndpoints(this WebApplication app)
    {
        // Keep full route templates: prefixed groups append a slash to collection routes.
        var investigationsGroup = app.MapGroup("");
        investigationsGroup.MapOpenFraudInvestigation();
        investigationsGroup.MapGetFraudInvestigation();
        investigationsGroup.MapRequestFraudReview();
        investigationsGroup.MapAttachFraudEvidence();
        investigationsGroup.MapRecordFraudDecision();
    }
}
