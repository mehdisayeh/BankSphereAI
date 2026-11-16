namespace BankSphere.Identity.Application;

public static class RefreshTokenCommandValidator
{
    public static void Validate(RefreshTokenCommand value)
    {
        ArgumentNullException.ThrowIfNull(value); ArgumentNullException.ThrowIfNull(value.Principal);
    }
}
