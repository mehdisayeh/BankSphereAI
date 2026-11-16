using System.Net;

namespace BankSphere.Identity.IntegrationTests;

public sealed class BrowserCookies(HttpMessageHandler inner) : DelegatingHandler(inner)
{
    private readonly CookieContainer cookies = new();
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri!;
        request.Headers.Remove("Cookie");
        var value = cookies.GetCookieHeader(uri);
        if (value.Length != 0) request.Headers.TryAddWithoutValidation("Cookie", value);
        var response = await base.SendAsync(request, cancellationToken);
        if (response.Headers.TryGetValues("Set-Cookie", out var headers))
            foreach (var header in headers) cookies.SetCookies(uri, header);
        return response;
    }
}
