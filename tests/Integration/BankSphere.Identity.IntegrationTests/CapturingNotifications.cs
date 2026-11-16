using System.Collections.Concurrent;
using BankSphere.Identity.Application;

namespace BankSphere.Identity.IntegrationTests;

public sealed class CapturingNotifications : IIdentityNotificationSender
{
    private readonly ConcurrentDictionary<(string Email, string Purpose), string> messages = new();
    public Task SendAsync(string email, string purpose, string encodedToken, CancellationToken cancellationToken)
    {
        messages[(email, purpose)] = encodedToken;
        return Task.CompletedTask;
    }
    public string Token(string email, string purpose) => messages[(email, purpose)];
}
