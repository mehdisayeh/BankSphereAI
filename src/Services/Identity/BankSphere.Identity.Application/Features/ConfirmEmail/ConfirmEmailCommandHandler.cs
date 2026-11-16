namespace BankSphere.Identity.Application;

public sealed class ConfirmEmailCommandHandler(IIdentityService identity)
{
    public Task Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        ConfirmEmailCommandValidator.Validate(command);
        return identity.ConfirmEmailAsync(command.Email, command.Token, cancellationToken);
    }
}
