namespace BankSphere.Identity.Application;

public sealed class LoginCommandHandler(IIdentityService identity)
{
    public Task<Guid> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        LoginCommandValidator.Validate(command);
        return identity.LoginAsync(command.Email, command.Password, command.AuthenticatorCode, cancellationToken);
    }
}
