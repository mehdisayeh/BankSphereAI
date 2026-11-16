namespace BankSphere.Identity.Application;

public sealed record UserProfile(Guid Id, string Email, string DisplayName, Guid? CustomerId, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
