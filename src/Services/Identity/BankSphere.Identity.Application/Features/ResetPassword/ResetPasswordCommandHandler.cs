namespace BankSphere.Identity.Application;

public sealed class ResetPasswordCommandHandler(IIdentityService identity)
{
    public Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        ResetPasswordCommandValidator.Validate(command);
        return identity.ResetPasswordAsync(command.Email, command.Token, command.NewPassword, cancellationToken);
    }
}
