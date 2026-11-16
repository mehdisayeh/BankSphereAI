using BankSphere.Identity.Api.Extensions;
using BankSphere.Identity.Application;
using BankSphere.Identity.Infrastructure;
using BankSphere.Observability;

var grantRole = args.Contains("--grant-role", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(argument => argument != "--grant-role").ToArray());
builder.AddIdentityServices();
var app = builder.Build();
if (grantRole)
{
    await new IdentityInitializer(app.Services, app.Configuration).StartAsync(CancellationToken.None);
    await using (var scope = app.Services.CreateAsyncScope())
    {
        await scope.ServiceProvider.GetRequiredService<GrantRoleCommandHandler>().Handle(new(
            app.Configuration["Identity:Provisioning:Email"] ?? "", app.Configuration["Identity:Provisioning:Role"] ?? ""), CancellationToken.None);
    }
    await app.DisposeAsync();
    return;
}
app.UseIdentityApi();
app.MapBankSphereHealthChecks();
app.MapIdentityEndpoints();
app.Run();
