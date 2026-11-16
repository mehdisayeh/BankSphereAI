using System.Net;
using System.Net.Http.Json;
using BankSphere.CreditAssessment.Application;

namespace BankSphere.CreditAssessment.Infrastructure;

public sealed class CreditCustomerGateway(HttpClient client) : ICreditCustomerGateway
{
    public async Task<bool> ExistsAsync(Guid customer, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"/customers/{customer}/eligibility", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Eligibility>(cancellationToken);
        return result?.Exists == true;
    }
    private sealed record Eligibility(bool Exists);
}
