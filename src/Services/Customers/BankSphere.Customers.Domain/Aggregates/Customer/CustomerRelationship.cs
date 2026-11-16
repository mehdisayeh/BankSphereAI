
namespace BankSphere.Customers.Domain;

public sealed record CustomerRelationship(Guid Id, Guid RelatedCustomerId, string Type, DateTimeOffset EffectiveFromUtc, DateTimeOffset? EffectiveToUtc);
