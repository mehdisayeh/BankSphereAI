namespace BankSphere.Identity.Application;

public sealed record RefreshTokenCommand(System.Security.Claims.ClaimsPrincipal Principal, IReadOnlyList<string>? Scopes = null);
