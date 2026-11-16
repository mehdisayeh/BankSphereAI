using BankSphere.FraudInvestigation.Application;
using BankSphere.Security;

namespace BankSphere.FraudInvestigation.Api.Endpoints;

public static class RequestFraudReviewEndpoint
{
    public static void MapRequestFraudReview(this RouteGroupBuilder group)
    {
        group.MapPost("/investigations/{id:guid}/reviews", async (Guid id, RequestFraudReview command, RequestFraudReviewHandler handler, HttpContext http, CancellationToken ct) => Results.Ok(await handler.Handle(id, command, http.User.BankingActor(), ct))).RequireAuthorization("banking-operator");
    }
}
