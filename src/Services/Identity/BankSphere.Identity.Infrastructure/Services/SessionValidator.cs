using System.Security.Claims;
using BankSphere.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace BankSphere.Identity.Infrastructure;

public sealed class SessionValidator(IdentityDbContext database, UserManager<ApplicationUser> users,
    IOpenIddictAuthorizationManager authorizations, IdentityService identity, TimeProvider clock)
{
    public async Task<bool> ValidateAsync(ClaimsPrincipal principal, bool validateGrants = true, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(principal.FindFirstValue("sub"), out var userId) || principal.FindFirstValue("sid") is not { } sessionId) return false;
        var session = await database.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(value => value.Id == sessionId && value.UserId == userId, cancellationToken);
        var user = await users.FindByIdAsync(userId.ToString());
        if (session is null || !session.IsActive(clock.GetUtcNow()) || user is not { IsEnabled: true, EmailConfirmed: true }
            || await users.IsLockedOutAsync(user) || session.SecurityStamp != await users.GetSecurityStampAsync(user)) return false;
        var authorization = await authorizations.FindByIdAsync(sessionId, cancellationToken);
        if (authorization is null || !await authorizations.HasStatusAsync(authorization, OpenIddictConstants.Statuses.Valid, cancellationToken)) return false;
        if (!validateGrants) return true;
        var hasBanking = principal.FindAll("scope").Any(claim => claim.Value.Split(' ').Contains(BankingPermissions.Scope, StringComparer.Ordinal));
        var roles = hasBanking ? await users.GetRolesAsync(user) : [];
        var permissions = hasBanking ? await identity.PermissionsAsync(userId, cancellationToken) : [];
        return principal.FindAll("role").Select(claim => claim.Value).ToHashSet(StringComparer.Ordinal).SetEquals(roles)
            && principal.FindAll(BankingPermissions.Claim).Select(claim => claim.Value).ToHashSet(StringComparer.Ordinal).SetEquals(permissions);
    }
}

