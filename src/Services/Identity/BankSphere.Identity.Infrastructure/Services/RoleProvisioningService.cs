using BankSphere.Identity.Application;
using BankSphere.Security;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Identity;

namespace BankSphere.Identity.Infrastructure;

// Only invoked by the explicit deployment CLI, never registered as an HTTP endpoint.
public sealed class RoleProvisioningService(UserManager<ApplicationUser> users, IIdentityService identity, SecurityAuditWriter audit)
    : IRoleProvisioningService
{
    public async Task GrantAsync(string email, string role, CancellationToken cancellationToken)
    {
        Guard.Against(!BankingPermissions.All.Contains(role, StringComparer.Ordinal), "identity.role", "Only existing banking roles may be provisioned.");
        var user = await users.FindByEmailAsync(email.Trim());
        Guard.Against(user is not { EmailConfirmed: true, IsEnabled: true }, "identity.user", "Provisioning requires an enabled, confirmed account.");
        if (!(await users.AddToRoleAsync(user!, role)).Succeeded)
            throw new BusinessException("identity.role", "Role assignment failed.", FailureKind.Conflict);
        await identity.RevokeSessionsAsync(user!.Id, null, cancellationToken);
        await audit.WriteAsync(user.Id, "role.granted:" + role, true, cancellationToken);
    }
}
