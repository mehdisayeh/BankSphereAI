using System.Globalization;

namespace BankSphere.SharedKernel;

public static class Guard
{
    public static void Against(bool condition, string code, string message, FailureKind kind = FailureKind.Validation)
    {
        if (condition) throw new BusinessException(code, message, kind);
    }

    public static string Text(string? value, string name, int maximum = 250)
    {
        Against(string.IsNullOrWhiteSpace(value) || value.Length > maximum, "validation.text", $"{name} is required and must be at most {maximum} characters.");
        return value!.Trim();
    }

    public static Guid Id(Guid value, string name)
    {
        Against(value == Guid.Empty, "validation.id", $"{name} must be a nonempty UUID.");
        return value;
    }

    public static DateTimeOffset Utc(DateTimeOffset value)
    {
        Against(value == default || value.Offset != TimeSpan.Zero, "validation.utc", "A nonempty UTC timestamp is required.");
        return value;
    }

    public static T Defined<T>(T value) where T : struct, Enum
    {
        Against(!Enum.IsDefined(value), "validation.enum", $"Unsupported {typeof(T).Name}.");
        return value;
    }

    public static string Country(string value)
    {
        var code = Text(value, "Country", 2).ToUpperInvariant();
        Against(!CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name).TwoLetterISORegionName).Contains(code), "validation.country", "Unknown ISO country code.");
        return code;
    }
}
