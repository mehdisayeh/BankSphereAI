namespace BankSphere.Identity.Application;

public sealed class LogoutCommandHandler(IIdentityService identity)
{
    public Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        LogoutCommandValidator.Validate(command);
        return identity.RevokeSessionsAsync(command.UserId, command.SessionId, cancellationToken);
    }
}
