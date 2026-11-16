namespace BankSphere.Identity.Application;

public sealed class RegisterCommandHandler(IIdentityService identity)
{
    public Task Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        RegisterCommandValidator.Validate(command);
        return identity.RegisterAsync(command.Email, command.Password, command.DisplayName, cancellationToken);
    }
}
