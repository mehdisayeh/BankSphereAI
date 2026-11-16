using BankSphere.Identity.Domain;
using BankSphere.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BankSphere.Identity.Infrastructure;

public sealed class IdentityInitializer(IServiceProvider services, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        await database.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource=N'BankSphere.Identity.Catalog', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000;
            IF @result < 0 THROW 51000, 'Could not acquire the identity catalog initialization lock.', 1;
            """, cancellationToken);
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        foreach (var name in BankingPermissions.All)
        {
            if (!await roles.RoleExistsAsync(name))
            {
                var created = await roles.CreateAsync(new Role { Id = Guid.NewGuid(), Name = name });
                if (!created.Succeeded) throw new InvalidOperationException("Could not provision the existing banking role catalog.");
            }
            if (!await database.Permissions.AnyAsync(value => value.Name == name, cancellationToken)) database.Permissions.Add(new Permission(name));
            var role = (await roles.FindByNameAsync(name))!;
            if (!await database.RolePermissions.AnyAsync(value => value.RoleId == role.Id && value.PermissionName == name, cancellationToken))
                database.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionName = name });
            await database.SaveChangesAsync(cancellationToken);
        }
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        foreach (var client in configuration.GetSection("Identity:Clients").GetChildren())
        {
            var id = client["ClientId"] ?? throw new InvalidOperationException("An OIDC client ID is required.");
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = id, ClientType = ClientTypes.Public, ConsentType = ConsentTypes.Implicit,
                DisplayName = client["DisplayName"] ?? id
            };
            descriptor.Permissions.UnionWith([Permissions.Endpoints.Authorization, Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode, Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code, Permissions.Scopes.Profile, Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + BankingPermissions.Scope]);
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
            foreach (var redirect in client.GetSection("RedirectUris").GetChildren())
            {
                if (!Uri.TryCreate(redirect.Value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps
                    || !string.IsNullOrEmpty(uri.Fragment)) throw new InvalidOperationException("Client redirect URIs must use HTTPS and have no fragment.");
                descriptor.RedirectUris.Add(uri);
            }
            if (descriptor.RedirectUris.Count == 0) throw new InvalidOperationException("A registered redirect URI is required.");
            var application = await manager.FindByClientIdAsync(id, cancellationToken);
            if (application is null) await manager.CreateAsync(descriptor, cancellationToken);
            else await manager.UpdateAsync(application, descriptor, cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
