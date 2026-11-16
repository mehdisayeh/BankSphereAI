using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BankSphere.Banking.Tests;

public sealed class EndpointContractTests
{
    [Theory]
    [InlineData("Customers", "POST /customers banking-operator|GET /customers/{id:guid} banking-reader|PUT /customers/{id:guid}/contact banking-operator|GET /customers/{id:guid}/eligibility banking-reader")]
    [InlineData("Accounts", "POST /accounts banking-operator|GET /accounts/{id:guid} banking-reader")]
    [InlineData("Transactions", "POST /simulation/transactions simulator|GET /transactions/{id:guid} banking-reader|GET /accounts/{accountId:guid}/transactions banking-reader")]
    [InlineData("FraudInvestigation", "POST /investigations banking-operator|GET /investigations/{id:guid} banking-reader|POST /investigations/{id:guid}/reviews banking-operator|POST /investigations/{id:guid}/evidence banking-operator|POST /investigations/{id:guid}/decisions fraud-reviewer")]
    [InlineData("Compliance", "POST /compliance-cases compliance-officer|GET /compliance-cases/{id:guid} compliance-officer")]
    [InlineData("CreditAssessment", "POST /credit-applications banking-operator|GET /credit-applications/{id:guid}/status banking-reader")]
    public async Task RoutesMethodsPoliciesAndHandlerRegistrationsRemainCompatible(string context, string expected)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Banking"] = "Server=localhost;Database=ContractTests;Integrated Security=true;TrustServerCertificate=true"
        });
        Action<WebApplication> map;
        switch (context)
        {
            case "Customers":
                BankSphere.Customers.Api.Extensions.DependencyInjection.AddBankingServices(builder);
                map = BankSphere.Customers.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints;
                break;
            case "Accounts":
                BankSphere.Accounts.Api.Extensions.DependencyInjection.AddBankingServices(builder);
                map = BankSphere.Accounts.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints;
                break;
            case "Transactions":
                BankSphere.Transactions.Api.Extensions.DependencyInjection.AddBankingServices(builder);
                map = BankSphere.Transactions.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints;
                break;
            case "FraudInvestigation":
                BankSphere.FraudInvestigation.Api.Extensions.DependencyInjection.AddBankingServices(builder);
                map = BankSphere.FraudInvestigation.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints;
                break;
            case "Compliance":
                BankSphere.Compliance.Api.Extensions.DependencyInjection.AddBankingServices(builder);
                map = BankSphere.Compliance.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints;
                break;
            case "CreditAssessment":
                BankSphere.CreditAssessment.Api.Extensions.DependencyInjection.AddBankingServices(builder);
                map = BankSphere.CreditAssessment.Api.Extensions.EndpointRegistrationExtensions.MapBankingEndpoints;
                break;
            default: throw new ArgumentException("Unknown banking context.", nameof(context));
        }

        await using var app = builder.Build();
        map(app);
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).Cast<RouteEndpoint>().ToArray();
        var actual = endpoints.Select(endpoint =>
            $"{Assert.Single(endpoint.Metadata.GetRequiredMetadata<HttpMethodMetadata>().HttpMethods)} {endpoint.RoutePattern.RawText} {Assert.Single(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()).Policy}");
        Assert.Equal(expected.Split('|').Order(StringComparer.Ordinal), actual.Order(StringComparer.Ordinal));

        // Resolve every registered use-case handler, including its repository and HTTP gateway dependencies.
        using var scope = app.Services.CreateScope();
        var handlers = builder.Services.Where(service => service.ServiceType.Namespace == $"BankSphere.{context}.Application"
            && service.ServiceType.Name.EndsWith("Handler", StringComparison.Ordinal)).ToArray();
        Assert.Equal(endpoints.Length, handlers.Length);
        foreach (var handler in handlers) Assert.NotNull(scope.ServiceProvider.GetRequiredService(handler.ServiceType));
    }
}
