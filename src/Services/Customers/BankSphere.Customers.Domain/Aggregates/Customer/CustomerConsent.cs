
namespace BankSphere.Customers.Domain;

public sealed record CustomerConsent(Guid Id, string Type, string PolicyVersion, DateTimeOffset GrantedAtUtc, DateTimeOffset? WithdrawnAtUtc);
