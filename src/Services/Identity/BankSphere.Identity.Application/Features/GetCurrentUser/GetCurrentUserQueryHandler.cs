namespace BankSphere.Identity.Application;

public sealed class GetCurrentUserQueryHandler(IIdentityService identity)
{
    public Task<UserProfile> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        GetCurrentUserQueryValidator.Validate(query);
        return identity.ProfileAsync(query.UserId, cancellationToken);
    }
}
