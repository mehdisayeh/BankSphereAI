
namespace BankSphere.SharedKernel;

public abstract class BankingAggregate : AggregateRoot<Guid>
{
    protected BankingAggregate() : base(Guid.NewGuid()) { }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }

    protected void Initialize(DateTimeOffset now)
    {
        CreatedAtUtc = Guard.Utc(now);
        UpdatedAtUtc = now;
        Version = 1;
    }

    protected void Changed(DateTimeOffset now)
    {
        Guard.Utc(now);
        Guard.Against(now < UpdatedAtUtc, "audit.timestamp", "An update cannot predate the last change.");
        UpdatedAtUtc = now;
        Version++;
    }
}
