
namespace BankSphere.Customers.Application;

public sealed record DocumentDetails(Guid Id, string Type, string Country, DateOnly IssueDate, DateOnly ExpiryDate, bool Verified);
