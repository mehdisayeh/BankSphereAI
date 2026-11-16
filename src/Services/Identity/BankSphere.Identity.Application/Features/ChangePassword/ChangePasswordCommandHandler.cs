namespace BankSphere.Identity.Application;

public sealed class ChangePasswordCommandHandler(IIdentityService identity)
{
    public Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        ChangePasswordCommandValidator.Validate(command);
        return identity.ChangePasswordAsync(command.UserId, command.CurrentPassword, command.NewPassword, cancellationToken);
    }
}
