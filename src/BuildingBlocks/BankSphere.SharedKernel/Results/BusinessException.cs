
namespace BankSphere.SharedKernel;

public sealed class BusinessException(string code, string message, FailureKind kind = FailureKind.Validation)
    : Exception(message)
{
    public string Code { get; } = code;
    public FailureKind Kind { get; } = kind;
}
