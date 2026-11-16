using System.Globalization;
using System.Text.RegularExpressions;

namespace BankSphere.SharedKernel;

public sealed record Iban
{
    public Iban(string value)
    {
        Value = Guard.Text(value, "IBAN", 34).Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        Guard.Against(!Regex.IsMatch(Value, "^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)), "account.iban", "Invalid IBAN format.");
        var rearranged = Value[4..] + Value[..4];
        var remainder = 0;
        foreach (var character in rearranged)
        {
            var digits = char.IsAsciiLetterUpper(character) ? (character - 'A' + 10).ToString(CultureInfo.InvariantCulture) : character.ToString();
            foreach (var digit in digits) remainder = (remainder * 10 + digit - '0') % 97;
        }
        Guard.Against(remainder != 1, "account.iban", "Invalid IBAN checksum.");
    }
    public string Value { get; }
}
