using System.Net;
using System.Net.Http.Json;
using BankSphere.FraudInvestigation.Application;

namespace BankSphere.FraudInvestigation.Infrastructure;

public sealed class InvestigationTransactions(HttpClient client) : IInvestigationTransactions
{
    public async Task<TransactionFacts?> FindAsync(Guid transaction, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"/transactions/{transaction}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TransactionFacts>(cancellationToken);
    }
}
