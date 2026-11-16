namespace BankSphere.Identity.Application;

public sealed class RefreshTokenCommandHandler(IIdentityProtocol protocol)
{
    public Task<System.Security.Claims.ClaimsPrincipal> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        RefreshTokenCommandValidator.Validate(command);
        return protocol.ExchangeAsync(command.Principal, command.Scopes, cancellationToken);
    }
}
