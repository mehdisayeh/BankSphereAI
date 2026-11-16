
namespace BankSphere.Customers.Domain;

public sealed record CustomerAddress(Guid Id, string Type, string Street, string City, string? Region,
    string PostalCode, string Country, DateTimeOffset EffectiveFromUtc, DateTimeOffset? EffectiveToUtc);
