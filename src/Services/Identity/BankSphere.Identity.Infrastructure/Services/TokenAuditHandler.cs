using OpenIddict.Server;

namespace BankSphere.Identity.Infrastructure;

public sealed class TokenAuditHandler(SecurityAuditWriter audit) : IOpenIddictServerHandler<OpenIddictServerEvents.ApplyTokenResponseContext>
{
    public async ValueTask HandleAsync(OpenIddictServerEvents.ApplyTokenResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!string.IsNullOrEmpty(context.Response.Error))
            await audit.WriteAsync(null, "token.rejected", false, context.CancellationToken);
    }
}
