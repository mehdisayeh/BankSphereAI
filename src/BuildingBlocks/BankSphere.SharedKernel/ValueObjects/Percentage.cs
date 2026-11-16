
namespace BankSphere.SharedKernel;

public sealed record Percentage
{
    public Percentage(decimal value)
    {
        Guard.Against(value < 0 || value > 100, "validation.percentage", "Percentage must be between zero and 100.");
        Value = value;
    }
    public decimal Value { get; }
}
