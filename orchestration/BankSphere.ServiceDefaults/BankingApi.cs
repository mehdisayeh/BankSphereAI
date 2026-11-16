using System.Text.Json.Serialization;
using BankSphere.Security;
using BankSphere.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BankSphere.ServiceDefaults;

public static class BankingApi
{
    public static WebApplicationBuilder AddBankingApi(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddBankingSecurity(builder.Configuration);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<BearerForwardingHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return builder;
    }

    public static WebApplication UseBankingApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.Use(async (context, next) =>
        {
            var supplied = context.Request.Headers["X-Correlation-ID"].ToString();
            context.TraceIdentifier = Guid.TryParse(supplied, out var id) ? id.ToString("D") : Guid.NewGuid().ToString("D");
            context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
            try
            {
                await next(context);
                if (context.Response.StatusCode is 400 or 401 or 403 or 404 && !context.Response.HasStarted)
                    await Problem(context, context.Response.StatusCode,
                        context.Response.StatusCode is 401 or 403 ? "authorization.denied" : "request.invalid_or_missing",
                        "The request is invalid, missing, or does not satisfy authentication and authorization requirements.");
            }
            catch (BusinessException exception)
            {
                var status = exception.Kind switch { FailureKind.NotFound => 404, FailureKind.Conflict => 409, FailureKind.Forbidden => 403, _ => 400 };
                await Problem(context, status, exception.Code, exception.Message);
            }
            catch (ArgumentException)
            {
                await Problem(context, 400, "validation.request", "Required request values are missing or malformed.");
            }
            catch (BadHttpRequestException)
            {
                await Problem(context, 400, "validation.json", "The request body or parameters are malformed.");
            }
            catch (HttpRequestException)
            {
                await Problem(context, 503, "dependency.unavailable", "A required banking service is unavailable.");
            }
        });
        app.UseAuthentication(); app.UseAuthorization();
        return app;
    }

    private static Task Problem(HttpContext context, int status, string code, string detail) => Results.Problem(statusCode: status,
        title: code, detail: detail, extensions: new Dictionary<string, object?>
        {
            ["code"] = code, ["correlationId"] = context.TraceIdentifier,
            ["errors"] = status == 400 ? new Dictionary<string, string[]> { ["request"] = [detail] } : null
        }).ExecuteAsync(context);
}
