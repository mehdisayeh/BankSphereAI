namespace BankSphere.Identity.Application;

public interface IIdentityService
{
    Task RegisterAsync(string email, string password, string displayName, CancellationToken cancellationToken);
    Task<Guid> LoginAsync(string email, string password, string? authenticatorCode, CancellationToken cancellationToken);
    Task ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken);
    Task ResetPasswordAsync(string email, string token, string password, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    Task<UserProfile> ProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task RevokeSessionsAsync(Guid userId, string? sessionId, CancellationToken cancellationToken);
}
