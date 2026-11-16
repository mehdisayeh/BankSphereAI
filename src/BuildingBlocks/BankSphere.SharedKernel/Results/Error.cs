namespace BankSphere.SharedKernel;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716", Justification = "Error is the intentional C# domain vocabulary; cross-language consumption is not required.")]
public sealed record Error
{
    public Error(string code, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Code = code;
        Description = description;
    }

    public string Code { get; }
    public string Description { get; }
}
