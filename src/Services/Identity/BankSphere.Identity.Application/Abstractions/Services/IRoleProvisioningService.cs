namespace BankSphere.Identity.Application;

public interface IRoleProvisioningService
{
    Task GrantAsync(string email, string role, CancellationToken cancellationToken);
}
