using Microsoft.AspNetCore;
using BankSphere.Identity.Application;
using BankSphere.Identity.Api.Extensions;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace BankSphere.Identity.Api.Endpoints;

public static class TokenEndpoint
{
    public static void MapToken(this RouteGroupBuilder group)
    {
        group.MapPost("/connect/token", async (HttpContext http, ExchangeAuthorizationCodeCommandHandler codes, RefreshTokenCommandHandler refresh, CancellationToken ct) =>
        {
            var request = http.GetOpenIddictServerRequest()!;
            var ticket = await http.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (ticket.Principal is null) return IdentityEndpointHelpers.ProtocolError(OpenIddictConstants.Errors.InvalidGrant);
            try
            {
                var principal = request.IsAuthorizationCodeGrantType()
                    ? await codes.Handle(new(ticket.Principal), ct)
                    : await refresh.Handle(new(ticket.Principal, string.IsNullOrWhiteSpace(request.Scope) ? null : request.GetScopes()), ct);
                return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            catch (BusinessException) { return IdentityEndpointHelpers.ProtocolError(OpenIddictConstants.Errors.InvalidGrant); }
        }).RequireRateLimiting("identity-auth");
    }
}
