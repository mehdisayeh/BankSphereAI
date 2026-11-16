using System.Security.Claims;

namespace BankSphere.Identity.Application;

public interface IIdentityProtocol
{
    Task<ClaimsPrincipal> AuthorizeAsync(Guid userId, string clientId, IReadOnlyList<string> scopes, CancellationToken cancellationToken);
    Task<ClaimsPrincipal> ExchangeAsync(ClaimsPrincipal principal, IReadOnlyList<string>? scopes, CancellationToken cancellationToken);
}
