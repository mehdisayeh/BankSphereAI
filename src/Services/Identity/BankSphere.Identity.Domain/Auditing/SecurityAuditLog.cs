namespace BankSphere.Identity.Domain;

public sealed class SecurityAuditLog
{
    private SecurityAuditLog() { }
    public SecurityAuditLog(Guid? userId, string action, bool succeeded, DateTimeOffset now, string correlationId)
    {
        Id = Guid.NewGuid(); UserId = userId; Action = action; Succeeded = succeeded; OccurredAtUtc = now; CorrelationId = correlationId;
    }
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = "";
    public bool Succeeded { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string CorrelationId { get; private set; } = "";
}
