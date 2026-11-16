using Microsoft.AspNetCore.Authorization;

namespace BankSphere.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true && context.User.HasClaim(BankingPermissions.Claim, requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
