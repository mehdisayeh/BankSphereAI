using Microsoft.AspNetCore.Http;

namespace BankSphere.Security;

// Forward the authenticated user's credential only to explicitly configured, trusted banking APIs.
public sealed class BearerForwardingHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = accessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization)) request.Headers.TryAddWithoutValidation("Authorization", authorization);
        var correlation = accessor.HttpContext?.TraceIdentifier;
        if (correlation is not null) request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlation);
        return base.SendAsync(request, cancellationToken);
    }
}
