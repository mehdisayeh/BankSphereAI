using BankSphere.Observability;
using BankSphere.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.MapBankSphereHealthChecks();
app.MapGet("/", () => TypedResults.Ok(new { service = "BankSphere AI", status = "ready" }));
app.Run();
