

namespace BankSphere.SharedKernel;

public sealed class Result<T> where T : notnull
{
    private readonly T? value;

    private Result(T value) => this.value = value;
    private Result(Error error) => Error = error;

    public bool IsSuccess => Error is null;
    public Error? Error { get; }
    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("A failed result has no value.");

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000", Justification = "Factories preserve result invariants and make the value type explicit at the call site.")]
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(value);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000", Justification = "Factories preserve result invariants and make the value type explicit at the call site.")]
    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }
}
