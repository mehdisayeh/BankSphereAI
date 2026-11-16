
namespace BankSphere.Accounts.Domain;

public sealed record AccountRestriction(Guid Id, string Type, string Reason, DateTimeOffset EffectiveAtUtc, DateTimeOffset? ExpiresAtUtc, DateTimeOffset? RemovedAtUtc);
