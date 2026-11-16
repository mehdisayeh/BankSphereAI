using System.Text;
using BankSphere.Identity.Application;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace BankSphere.Identity.Infrastructure;

public sealed class IdentityService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    IdentityDbContext database, IIdentityNotificationSender notifications, SecurityAuditWriter audit, TimeProvider clock,
    IOpenIddictAuthorizationManager authorizations, IOpenIddictTokenManager tokens) : IIdentityService
{
    public async Task RegisterAsync(string email, string password, string displayName, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email.Trim(), Email = email.Trim(),
            DisplayName = displayName.Trim(), CreatedAtUtc = clock.GetUtcNow(), LockoutEnabled = true };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            if (result.Errors.All(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                var existing = await users.FindByEmailAsync(email.Trim());
                if (existing is { EmailConfirmed: false, IsEnabled: true })
                    await notifications.SendAsync(existing.Email!, "confirm-email", Encode(await users.GenerateEmailConfirmationTokenAsync(existing)), cancellationToken);
                return;
            }
            throw new BusinessException("identity.registration", "Registration failed. Check the email and password policy.");
        }
        // Deliberately no role assignment, Customer link, or client-supplied authorization metadata.
        var token = Encode(await users.GenerateEmailConfirmationTokenAsync(user));
        await audit.WriteAsync(user.Id, "registration.created", true, cancellationToken);
        await notifications.SendAsync(user.Email!, "confirm-email", token, cancellationToken);
    }

    public async Task<Guid> LoginAsync(string email, string password, string? authenticatorCode, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        var valid = false;
        if (user is { IsEnabled: true })
        {
            var result = await signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            valid = result.Succeeded;
            if (result.IsNotAllowed || result.IsLockedOut) _ = users.PasswordHasher.HashPassword(new ApplicationUser(), password);
            if (valid && user.TwoFactorEnabled)
            {
                valid = authenticatorCode is not null && await users.VerifyTwoFactorTokenAsync(user,
                    TokenOptions.DefaultAuthenticatorProvider, authenticatorCode);
                if (!valid) await users.AccessFailedAsync(user);
            }
        }
        else
        {
            // Comparable password-hashing work for unknown accounts; never return account existence.
            _ = users.PasswordHasher.HashPassword(new ApplicationUser(), password);
        }
        await audit.WriteAsync(user?.Id, "login", valid, cancellationToken);
        if (!valid || user is null) throw new BusinessException("identity.credentials", "Invalid credentials or account unavailable.", FailureKind.Forbidden);
        user.LastLoginAtUtc = clock.GetUtcNow();
        Ensure(await users.UpdateAsync(user));
        return user.Id;
    }

    public async Task ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || !(await users.ConfirmEmailAsync(user, Decode(token))).Succeeded)
            throw new BusinessException("identity.confirmation", "Invalid or expired confirmation.");
        await audit.WriteAsync(user.Id, "email.confirmed", true, cancellationToken);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        await audit.WriteAsync(user?.Id, "password.reset-requested", true, cancellationToken);
        if (user is { IsEnabled: true, EmailConfirmed: true })
            await notifications.SendAsync(user.Email!, "reset-password", Encode(await users.GeneratePasswordResetTokenAsync(user)), cancellationToken);
    }

    public async Task ResetPasswordAsync(string email, string token, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || !(await users.ResetPasswordAsync(user, Decode(token), password)).Succeeded)
        {
            await audit.WriteAsync(user?.Id, "password.reset", false, cancellationToken);
            throw new BusinessException("identity.reset", "Invalid reset request or password.");
        }
        user.PasswordChangedAtUtc = clock.GetUtcNow();
        Ensure(await users.UpdateAsync(user));
        await RevokeSessionsAsync(user.Id, null, cancellationToken);
        await audit.WriteAsync(user.Id, "password.reset", true, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await UserAsync(userId);
        if (!(await signIn.CheckPasswordSignInAsync(user, currentPassword, lockoutOnFailure: true)).Succeeded)
        {
            await audit.WriteAsync(userId, "password.changed", false, cancellationToken);
            throw new BusinessException("identity.password", "Invalid current password or account unavailable.", FailureKind.Forbidden);
        }
        var result = await users.ChangePasswordAsync(user, currentPassword, newPassword);
        await audit.WriteAsync(userId, "password.changed", result.Succeeded, cancellationToken);
        if (!result.Succeeded) throw new BusinessException("identity.password", "Invalid current password or new password policy.");
        user.PasswordChangedAtUtc = clock.GetUtcNow();
        Ensure(await users.UpdateAsync(user));
        await RevokeSessionsAsync(userId, null, cancellationToken);
    }

    public async Task<UserProfile> ProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await UserAsync(userId);
        return new(user.Id, user.Email!, user.DisplayName, user.CustomerId, (await users.GetRolesAsync(user)).ToArray(),
            await PermissionsAsync(userId, cancellationToken));
    }

    public async Task<string[]> PermissionsAsync(Guid userId, CancellationToken cancellationToken) => await (
        from membership in database.UserRoles where membership.UserId == userId
        join grant in database.RolePermissions on membership.RoleId equals grant.RoleId
        select grant.PermissionName).Distinct().OrderBy(value => value).ToArrayAsync(cancellationToken);

    public async Task RevokeSessionsAsync(Guid userId, string? sessionId, CancellationToken cancellationToken)
    {
        var sessions = await database.RefreshTokens.Where(session => session.UserId == userId
            && (sessionId == null || session.Id == sessionId)).ToListAsync(cancellationToken);
        if (sessionId is not null && sessions.Count == 0)
            throw new BusinessException("identity.session", "Session does not exist.", FailureKind.NotFound);
        foreach (var session in sessions)
        {
            session.Revoke(clock.GetUtcNow(), "user-revoked");
            var authorization = await authorizations.FindByIdAsync(session.Id, cancellationToken);
            if (authorization is not null) await authorizations.TryRevokeAsync(authorization, cancellationToken);
            await tokens.RevokeByAuthorizationIdAsync(session.Id, cancellationToken);
        }
        if (sessionId is null) Ensure(await users.UpdateSecurityStampAsync(await UserAsync(userId)));
        await audit.WriteAsync(userId, "sessions.revoked", true, cancellationToken);
    }

    private async Task<ApplicationUser> UserAsync(Guid userId) => await users.FindByIdAsync(userId.ToString())
        ?? throw new BusinessException("identity.user", "User unavailable.", FailureKind.NotFound);
    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new BusinessException("identity.concurrent-update", "Identity changed; retry the request.", FailureKind.Conflict);
    }
    private static string Encode(string value) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(value));
    private static string Decode(string value)
    {
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value));
        }
        catch (FormatException)
        {
            throw new BusinessException("identity.token", "Invalid or expired token.");
        }
    }
}
