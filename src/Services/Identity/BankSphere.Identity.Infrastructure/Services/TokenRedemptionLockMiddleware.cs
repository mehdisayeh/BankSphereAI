using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BankSphere.Identity.Infrastructure;

// Serialize the full validation/redemption/issuance pipeline across all instances sharing this database.
// Only a SHA-256 digest is used as the SQL lock name; credentials and token values are never persisted here.
public sealed class TokenRedemptionLockMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext http)
    {
        if (http.Request.Path.Value?.TrimEnd('/').Equals("/connect/token", StringComparison.OrdinalIgnoreCase) != true || !http.Request.HasFormContentType)
        {
            await next(http); return;
        }
        http.Response.Headers.CacheControl = "no-store";
        http.Response.Headers.Pragma = "no-cache";
        IFormCollection form;
        try { form = await http.Request.ReadFormAsync(http.RequestAborted); }
        catch (InvalidDataException)
        {
            await Results.BadRequest(new { error = "invalid_request" }).ExecuteAsync(http); return;
        }
        var credential = form["grant_type"].ToString() switch
        {
            "authorization_code" => form["code"].ToString(),
            "refresh_token" => form["refresh_token"].ToString(), _ => ""
        };
        if (credential.Length == 0) { await next(http); return; }
        var resource = "BankSphere.Identity.Redeem:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential)));
        // Separate bounded pool prevents lock waiters from consuming the EF store's connection pool.
        var settings = new SqlConnectionStringBuilder(configuration.GetConnectionString("Banking"))
            { ApplicationName = "BankSphere.Identity.RedemptionLocks", MaxPoolSize = 32, ConnectTimeout = 5 };
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(http.RequestAborted);
        await using var acquire = connection.CreateCommand();
        acquire.CommandText = """
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource=@resource, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=5000;
            SELECT @result;
            """;
        acquire.Parameters.Add("@resource", SqlDbType.NVarChar, 255).Value = resource;
        var result = Convert.ToInt32(await acquire.ExecuteScalarAsync(http.RequestAborted), System.Globalization.CultureInfo.InvariantCulture);
        if (result < 0)
        {
            await Results.Json(new { error = "temporarily_unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable).ExecuteAsync(http);
            return;
        }
        try { await next(http); }
        finally
        {
            await using var release = connection.CreateCommand();
            release.CommandTimeout = 5;
            release.CommandText = "EXEC sys.sp_releaseapplock @Resource=@resource, @LockOwner='Session';";
            release.Parameters.Add("@resource", SqlDbType.NVarChar, 255).Value = resource;
            await release.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }
}
