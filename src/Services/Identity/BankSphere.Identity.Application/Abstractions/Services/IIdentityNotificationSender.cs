namespace BankSphere.Identity.Application;

public interface IIdentityNotificationSender
{
    Task SendAsync(string email, string purpose, string encodedToken, CancellationToken cancellationToken);
}
