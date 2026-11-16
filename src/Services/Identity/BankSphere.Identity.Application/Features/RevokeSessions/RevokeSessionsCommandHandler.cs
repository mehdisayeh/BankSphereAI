namespace BankSphere.Identity.Application;

public sealed class RevokeSessionsCommandHandler(IIdentityService identity)
{
    public Task Handle(RevokeSessionsCommand command, CancellationToken cancellationToken)
    {
        RevokeSessionsCommandValidator.Validate(command);
        return identity.RevokeSessionsAsync(command.UserId, command.SessionId, cancellationToken);
    }
}
