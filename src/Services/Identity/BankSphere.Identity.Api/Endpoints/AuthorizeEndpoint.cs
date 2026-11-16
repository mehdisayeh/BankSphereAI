using Microsoft.AspNetCore;
using BankSphere.Identity.Application;
using BankSphere.Identity.Infrastructure;
using BankSphere.Identity.Api.Extensions;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace BankSphere.Identity.Api.Endpoints;

public static class AuthorizeEndpoint
{
    public static void MapAuthorize(this RouteGroupBuilder group)
    {
        group.MapGet("/connect/authorize", async (HttpContext http, SignInManager<ApplicationUser> signIn, AuthorizeCommandHandler handler, CancellationToken ct) =>
        {
            var request = http.GetOpenIddictServerRequest()!;
            var cookie = await http.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (!cookie.Succeeded || cookie.Principal is null || await signIn.ValidateSecurityStampAsync(cookie.Principal) is not { } user)
                return IdentityEndpointHelpers.ProtocolError(OpenIddictConstants.Errors.LoginRequired);
            // This API-only provider uses an external same-origin account UI. Never silently
            // satisfy an explicit reauthentication/consent/account-selection request with an old cookie.
            if (request.HasPromptValue(OpenIddictConstants.PromptValues.Login)
                || request.HasPromptValue(OpenIddictConstants.PromptValues.Consent)
                || request.HasPromptValue(OpenIddictConstants.PromptValues.SelectAccount))
                return IdentityEndpointHelpers.ProtocolError(OpenIddictConstants.Errors.InteractionRequired);
            if (request.MaxAge is { } maximumAge && (cookie.Properties?.IssuedUtc is not { } authenticatedAt
                || (DateTimeOffset.UtcNow - authenticatedAt).TotalSeconds >= maximumAge))
                return IdentityEndpointHelpers.ProtocolError(OpenIddictConstants.Errors.LoginRequired);
            try
            {
                var principal = await handler.Handle(new(user.Id, request.ClientId!, request.GetScopes()), ct);
                var issued = cookie.Properties?.IssuedUtc ?? DateTimeOffset.UtcNow;
                ((System.Security.Claims.ClaimsIdentity)principal.Identity!).AddClaim(new System.Security.Claims.Claim(
                    OpenIddictConstants.Claims.AuthenticationTime, issued.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
                    System.Security.Claims.ClaimValueTypes.Integer64).SetDestinations(OpenIddictConstants.Destinations.IdentityToken));
                return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            catch (BusinessException) { return IdentityEndpointHelpers.ProtocolError(OpenIddictConstants.Errors.AccessDenied); }
        }).RequireRateLimiting("identity-auth");
    }
}
