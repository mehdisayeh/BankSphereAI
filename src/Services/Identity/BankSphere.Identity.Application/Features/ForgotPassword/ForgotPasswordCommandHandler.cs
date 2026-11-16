namespace BankSphere.Identity.Application;

public sealed class ForgotPasswordCommandHandler(IIdentityService identity)
{
    public Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        ForgotPasswordCommandValidator.Validate(command);
        return identity.ForgotPasswordAsync(command.Email, cancellationToken);
    }
}
