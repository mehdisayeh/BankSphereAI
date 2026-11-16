
namespace BankSphere.SharedKernel;

public sealed record DateRange
{
    public DateRange(DateTimeOffset fromUtc, DateTimeOffset? toUtc)
    {
        FromUtc = Guard.Utc(fromUtc);
        ToUtc = toUtc is null ? null : Guard.Utc(toUtc.Value);
        Guard.Against(toUtc < fromUtc, "validation.dates", "End must follow start.");
    }
    public DateTimeOffset FromUtc { get; }
    public DateTimeOffset? ToUtc { get; }
}
