using BankSphere.Identity.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BankSphere.Identity.Api.Extensions;

public sealed class IdentityDatabaseHealthCheck(IServiceScopeFactory scopes) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using var scope = scopes.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        return await database.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
