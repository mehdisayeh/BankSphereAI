

namespace BankSphere.SharedKernel;

// Identity is explicit; equality remains reference equality until a domain needs otherwise.
public abstract class Entity<TId> where TId : notnull
{
    protected Entity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    public TId Id { get; private set; }
}
