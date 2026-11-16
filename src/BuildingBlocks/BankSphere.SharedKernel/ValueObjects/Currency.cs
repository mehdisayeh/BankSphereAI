
namespace BankSphere.SharedKernel;

public sealed record Currency
{
    // Supported ISO 4217 currencies. Unsupported currencies fail closed; extend by reviewed policy.
    private static readonly Dictionary<string, int> MinorUnits = new(StringComparer.Ordinal)
    {
        ["EUR"] = 2, ["USD"] = 2, ["GBP"] = 2, ["CHF"] = 2, ["CAD"] = 2,
        ["AUD"] = 2, ["JPY"] = 0, ["KWD"] = 3, ["BHD"] = 3, ["TND"] = 3
    };

    public Currency(string code)
    {
        Code = Guard.Text(code, "Currency", 3).ToUpperInvariant();
        Guard.Against(!MinorUnits.ContainsKey(Code), "money.currency", "Currency is not in the supported ISO 4217 catalog.");
    }

    public string Code { get; }
    public int FractionDigits => MinorUnits[Code];
}
