using System.Security.Claims;
using BankSphere.Identity.Application;
using BankSphere.Identity.Domain;
using BankSphere.Security;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BankSphere.Identity.Infrastructure;

public sealed class IdentityProtocol(UserManager<ApplicationUser> users, IOpenIddictApplicationManager applications,
    IOpenIddictAuthorizationManager authorizations, IdentityDbContext database, IdentityService identity,
    SessionValidator sessions, SecurityAuditWriter audit, TimeProvider clock, IConfiguration configuration) : IIdentityProtocol
{
    public async Task<ClaimsPrincipal> AuthorizeAsync(Guid userId, string clientId, IReadOnlyList<string> scopes, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is not { IsEnabled: true, EmailConfirmed: true } || await users.IsLockedOutAsync(user))
            throw new BusinessException("identity.authorization", "User unavailable.", FailureKind.Forbidden);
        var application = await applications.FindByClientIdAsync(clientId, cancellationToken)
            ?? throw new BusinessException("identity.client", "Client unavailable.", FailureKind.Forbidden);
        var principal = await PrincipalAsync(user, scopes, cancellationToken);
        var authorization = await authorizations.CreateAsync(principal, userId.ToString(),
            (await applications.GetIdAsync(application, cancellationToken))!, AuthorizationTypes.AdHoc, principal.GetScopes(), cancellationToken);
        var id = (await authorizations.GetIdAsync(authorization, cancellationToken))!;
        principal.SetAuthorizationId(id);
        principal.SetClaim("sid", id);
        principal.SetDestinations(claim => claim.Type is "sub" or "sid" or "name" or "role" or "permission"
            ? [Destinations.AccessToken] : []);
        database.RefreshTokens.Add(new RefreshToken(id, userId, clientId, await users.GetSecurityStampAsync(user), clock.GetUtcNow()));
        await audit.WriteAsync(userId, "authorization.created", true, cancellationToken);
        return principal;
    }

    public async Task<ClaimsPrincipal> ExchangeAsync(ClaimsPrincipal principal, IReadOnlyList<string>? scopes, CancellationToken cancellationToken)
    {
        if (!await sessions.ValidateAsync(principal, validateGrants: false, cancellationToken))
            throw new BusinessException("identity.session", "Session expired or revoked.", FailureKind.Forbidden);
        var user = (await users.FindByIdAsync(principal.GetClaim(Claims.Subject)!))!;
        var grantedScopes = scopes is null ? principal.GetScopes().AsEnumerable() : principal.GetScopes().Intersect(scopes, StringComparer.Ordinal);
        var result = await PrincipalAsync(user, grantedScopes, cancellationToken);
        result.SetAuthorizationId(principal.GetAuthorizationId());
        result.SetPresenters(principal.GetPresenters());
        result.SetClaim("sid", principal.GetClaim("sid"));
        result.SetDestinations(claim => claim.Type is "sub" or "sid" or "name" or "role" or "permission" ? [Destinations.AccessToken] : []);
        if (principal.FindFirst(Claims.AuthenticationTime) is { } authenticationTime)
            ((ClaimsIdentity)result.Identity!).AddClaim(new Claim(authenticationTime.Type, authenticationTime.Value, authenticationTime.ValueType)
                .SetDestinations(Destinations.IdentityToken));
        var session = await database.RefreshTokens.SingleAsync(value => value.Id == principal.GetClaim("sid"), cancellationToken);
        session.Use(clock.GetUtcNow());
        // Absolute family lifetime prevents rolling refresh tokens from extending a session forever.
        result.SetRefreshTokenLifetime(session.ExpiresAtUtc - clock.GetUtcNow());
        await audit.WriteAsync(user.Id, "token.exchanged", true, cancellationToken);
        return result;
    }

    private async Task<ClaimsPrincipal> PrincipalAsync(ApplicationUser user, IEnumerable<string> scopes, CancellationToken cancellationToken)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity("OpenIddict", Claims.Name, Claims.Role));
        principal.SetClaim(Claims.Subject, user.Id.ToString());
        principal.SetClaim(Claims.Name, user.DisplayName);
        principal.SetScopes(scopes);
        principal.SetResources(configuration["Authentication:Audience"] ?? BankingSecurity.DefaultAudience);
        if (principal.HasScope(BankingPermissions.Scope))
        {
            foreach (var role in await users.GetRolesAsync(user)) ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(Claims.Role, role));
            foreach (var permission in await identity.PermissionsAsync(user.Id, cancellationToken))
                ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(BankingPermissions.Claim, permission));
        }
        return principal;
    }
}
