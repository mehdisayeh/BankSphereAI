namespace BankSphere.Identity.Application;

public static class AuthorizeCommandValidator
{
    public static void Validate(AuthorizeCommand value)
    {
        ArgumentNullException.ThrowIfNull(value); BankSphere.SharedKernel.Guard.Id(value.UserId, "User"); BankSphere.SharedKernel.Guard.Text(value.ClientId, "Client", 100); ArgumentNullException.ThrowIfNull(value.Scopes);
    }
}
