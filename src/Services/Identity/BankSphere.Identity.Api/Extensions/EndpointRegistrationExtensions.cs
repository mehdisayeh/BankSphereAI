using BankSphere.Identity.Api.Endpoints;

namespace BankSphere.Identity.Api.Extensions;

public static class EndpointRegistrationExtensions
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        var identity = app.MapGroup("");
        identity.MapAntiforgery();
        identity.MapRegister();
        identity.MapLogin();
        identity.MapConfirmEmail();
        identity.MapForgotPassword();
        identity.MapResetPassword();
        identity.MapChangePassword();
        identity.MapGetCurrentUser();
        identity.MapRevokeSessions();
        identity.MapLogout();
        identity.MapValidateSession();
        identity.MapAuthorize();
        identity.MapToken();
    }
}
