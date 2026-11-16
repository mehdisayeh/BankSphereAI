using System.Security.Claims;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Server.AspNetCore;

namespace BankSphere.Identity.Api.Extensions;

public static class IdentityEndpointHelpers
{
    public static Guid UserId(HttpContext http) => Guid.TryParse(http.User.FindFirstValue("sub"), out var id)
        ? id : throw new BusinessException("identity.subject", "Invalid subject.", FailureKind.Forbidden);
    public static string SessionId(HttpContext http) => http.User.FindFirstValue("sid")
        ?? throw new BusinessException("identity.session", "Invalid session.", FailureKind.Forbidden);
    public static async Task ValidateCsrfAsync(HttpContext http, IAntiforgery antiforgery)
    {
        try { await antiforgery.ValidateRequestAsync(http); }
        catch (AntiforgeryValidationException) { throw new BusinessException("identity.csrf", "An anti-forgery token is required."); }
    }
    public static IResult ProtocolError(string error) => Results.Forbid(new AuthenticationProperties(new Dictionary<string, string?>
    {
        [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Authentication or session validation failed."
    }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
}
