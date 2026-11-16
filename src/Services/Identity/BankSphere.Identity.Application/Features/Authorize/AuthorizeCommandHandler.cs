namespace BankSphere.Identity.Application;

public sealed class AuthorizeCommandHandler(IIdentityProtocol protocol)
{
    public Task<System.Security.Claims.ClaimsPrincipal> Handle(AuthorizeCommand command, CancellationToken cancellationToken)
    {
        AuthorizeCommandValidator.Validate(command);
        return protocol.AuthorizeAsync(command.UserId, command.ClientId, command.Scopes, cancellationToken);
    }
}
