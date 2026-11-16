using BankSphere.SharedKernel;

namespace BankSphere.Accounts.Domain;

public sealed class AccountBalanceSnapshot
{
    private AccountBalanceSnapshot() { }
    public Money BookedBalance { get; private set; } = null!;
    public Money AvailableBalance { get; private set; } = null!;
    public Money PendingAmount { get; private set; } = null!;
    public DateTimeOffset AsOfUtc { get; private set; }
    public string LedgerReference { get; private set; } = "";
    public BalanceSource Source { get; private set; }

    public static AccountBalanceSnapshot FromProjection(Money booked, Money available, Money pending,
        DateTimeOffset asOfUtc, string reference, BalanceSource source)
    {
        booked.Add(available).Add(pending);
        return new() { BookedBalance = booked, AvailableBalance = available, PendingAmount = pending,
            AsOfUtc = Guard.Utc(asOfUtc), LedgerReference = Guard.Text(reference, "Projection reference"), Source = Guard.Defined(source) };
    }
}
