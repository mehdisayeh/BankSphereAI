

namespace BankSphere.SharedKernel;

public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id) where TId : notnull
{
    private readonly List<IDomainEvent> domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        domainEvents.Add(domainEvent);
    }

    // The future unit of work must clear only after durable persistence/outbox capture.
    public void ClearDomainEvents() => domainEvents.Clear();
}
