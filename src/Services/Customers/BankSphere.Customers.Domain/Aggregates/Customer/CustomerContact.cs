
namespace BankSphere.Customers.Domain;

public sealed record CustomerContact(Guid Id, string Type, string Value, bool Verified);
