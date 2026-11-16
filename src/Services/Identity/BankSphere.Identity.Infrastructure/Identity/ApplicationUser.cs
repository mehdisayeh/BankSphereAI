using Microsoft.AspNetCore.Identity;

namespace BankSphere.Identity.Infrastructure;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = "";
    // Explicit optional reference; no Customer aggregate/navigation or cross-schema FK.
    public Guid? CustomerId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }
    public DateTimeOffset? PasswordChangedAtUtc { get; set; }
}
