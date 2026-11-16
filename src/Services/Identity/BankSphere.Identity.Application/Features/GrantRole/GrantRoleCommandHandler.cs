namespace BankSphere.Identity.Application;

public sealed class GrantRoleCommandHandler(IRoleProvisioningService provisioning)
{
    public Task Handle(GrantRoleCommand command, CancellationToken cancellationToken)
    {
        GrantRoleCommandValidator.Validate(command);
        return provisioning.GrantAsync(command.Email, command.Role, cancellationToken);
    }
}
