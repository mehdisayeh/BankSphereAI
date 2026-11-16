using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace BankSphere.Identity.Infrastructure;

// Runs after cryptographic validation and before OpenIddict's token-entry validation.
// OpenIddict revokes redeemed-token chains; also close our session so signed access JWTs fail immediately.
public sealed class TokenReuseHandler(IOpenIddictTokenManager tokens, IOpenIddictAuthorizationManager authorizations,
    IdentityDbContext database, SecurityAuditWriter audit, TimeProvider clock)
    : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateTokenContext>
{
    public async ValueTask HandleAsync(OpenIddictServerEvents.ValidateTokenContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.TokenId is null || context.Principal is null
            || (!context.Principal.HasTokenType(OpenIddictConstants.TokenTypeIdentifiers.RefreshToken)
                && !context.Principal.HasTokenType(OpenIddictConstants.TokenTypeIdentifiers.Private.AuthorizationCode))) return;
        var token = await tokens.FindByIdAsync(context.TokenId, context.CancellationToken);
        if (token is null || !await tokens.HasStatusAsync(token, OpenIddictConstants.Statuses.Redeemed, context.CancellationToken)) return;
        var id = context.Principal.GetAuthorizationId();
        if (id is null) return;
        var authorization = await authorizations.FindByIdAsync(id, context.CancellationToken);
        if (authorization is not null) await authorizations.TryRevokeAsync(authorization, context.CancellationToken);
        var session = await database.RefreshTokens.SingleOrDefaultAsync(value => value.Id == id, context.CancellationToken);
        var refresh = context.Principal.HasTokenType(OpenIddictConstants.TokenTypeIdentifiers.RefreshToken);
        session?.Revoke(clock.GetUtcNow(), refresh ? "refresh-token-reuse" : "authorization-code-reuse");
        await audit.WriteAsync(session?.UserId, refresh ? "refresh.reuse-detected" : "code.reuse-detected", false, context.CancellationToken);
    }
}
