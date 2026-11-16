using BankSphere.Identity.Domain;
using Microsoft.AspNetCore.Http;

namespace BankSphere.Identity.Infrastructure;

public sealed class SecurityAuditWriter(IdentityDbContext database, TimeProvider clock, IHttpContextAccessor http)
{
    public async Task WriteAsync(Guid? userId, string action, bool succeeded, CancellationToken cancellationToken = default)
    {
        database.SecurityAuditLogs.Add(new SecurityAuditLog(userId, action, succeeded, clock.GetUtcNow(),
            http.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N")));
        await database.SaveChangesAsync(cancellationToken);
    }
}
