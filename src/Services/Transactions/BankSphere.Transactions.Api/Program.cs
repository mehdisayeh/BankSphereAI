using BankSphere.Transactions.Api.Extensions;
using BankSphere.Observability;
using BankSphere.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddBankingServices();

var app = builder.Build();
app.UseBankingApi();
app.MapBankSphereHealthChecks();
app.MapBankingEndpoints();
app.Run();
