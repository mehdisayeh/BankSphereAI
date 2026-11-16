namespace BankSphere.Identity.Application;

public sealed class ExchangeAuthorizationCodeCommandHandler(IIdentityProtocol protocol)
{
    public Task<System.Security.Claims.ClaimsPrincipal> Handle(ExchangeAuthorizationCodeCommand command, CancellationToken cancellationToken)
    {
        ExchangeAuthorizationCodeCommandValidator.Validate(command);
        return protocol.ExchangeAsync(command.Principal, null, cancellationToken);
    }
}
