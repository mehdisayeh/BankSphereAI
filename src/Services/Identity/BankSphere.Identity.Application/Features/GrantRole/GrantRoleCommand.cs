namespace BankSphere.Identity.Application;

public sealed record GrantRoleCommand(string Email, string Role);
