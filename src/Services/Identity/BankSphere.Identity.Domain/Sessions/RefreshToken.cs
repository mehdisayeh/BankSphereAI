namespace BankSphere.Identity.Domain;

// Refresh-family/session metadata only. OpenIddict owns encrypted token payloads, redemption and rotation.
public sealed class RefreshToken
{
    private RefreshToken() { }
    public RefreshToken(string id, Guid userId, string clientId, string securityStamp, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id); ArgumentException.ThrowIfNullOrWhiteSpace(securityStamp);
        Id = id; UserId = userId; ClientId = clientId; SecurityStamp = securityStamp;
        CreatedAtUtc = now; ExpiresAtUtc = now.AddDays(30); LastUsedAtUtc = now;
    }
    public string Id { get; private set; } = "";
    public Guid UserId { get; private set; }
    public string ClientId { get; private set; } = "";
    public string SecurityStamp { get; private set; } = "";
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset LastUsedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public string? RevocationReason { get; private set; }
    public bool IsActive(DateTimeOffset now) => RevokedAtUtc is null && ExpiresAtUtc > now;
    public void Use(DateTimeOffset now)
    {
        if (!IsActive(now)) throw new InvalidOperationException("Session is inactive.");
        LastUsedAtUtc = now;
    }
    public void Revoke(DateTimeOffset now, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        RevokedAtUtc ??= now; RevocationReason ??= reason;
    }
}
