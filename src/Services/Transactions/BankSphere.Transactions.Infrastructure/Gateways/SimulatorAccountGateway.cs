using System.Net;
using System.Net.Http.Json;
using BankSphere.Transactions.Application;

namespace BankSphere.Transactions.Infrastructure;

public sealed class SimulatorAccountGateway(HttpClient client) : ISimulatorAccountGateway
{
    public async Task<bool> IsOwnedByAsync(Guid account, Guid customer, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"/accounts/{account}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AccountOwnershipResponse>(cancellationToken);
        return result?.PrimaryOwnerId == customer;
    }
    private sealed record AccountOwnershipResponse(Guid PrimaryOwnerId);
}
