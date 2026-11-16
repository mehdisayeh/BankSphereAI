namespace BankSphere.Identity.Application;

public sealed record AuthorizeCommand(Guid UserId, string ClientId, IReadOnlyList<string> Scopes);
