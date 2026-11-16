namespace BankSphere.Identity.Application;

public static class ExchangeAuthorizationCodeCommandValidator
{
    public static void Validate(ExchangeAuthorizationCodeCommand value)
    {
        ArgumentNullException.ThrowIfNull(value); ArgumentNullException.ThrowIfNull(value.Principal);
    }
}
