
namespace BankSphere.Accounts.Domain;

public sealed record AccountMandate(Guid Id, Guid AuthorizedPersonId, string Type, string Permissions, DateTimeOffset ExpirationUtc, DateTimeOffset? RevokedAtUtc);
