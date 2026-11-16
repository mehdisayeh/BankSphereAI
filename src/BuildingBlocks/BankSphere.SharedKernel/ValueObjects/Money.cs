
namespace BankSphere.SharedKernel;

public sealed record Money
{
    public Money(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        Guard.Against(amount < 0 || amount > 999999999999999.9999m || decimal.Round(amount, currency.FractionDigits) != amount,
            "money.amount", "Amount must be nonnegative, within decimal(19,4), and respect currency minor units.");
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        Guard.Against(Currency != other.Currency, "money.currency_mismatch", "Amounts must use the same currency.");
        return new Money(Amount + other.Amount, Currency);
    }
}
