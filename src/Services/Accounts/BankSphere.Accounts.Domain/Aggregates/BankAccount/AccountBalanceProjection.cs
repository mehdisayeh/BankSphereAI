

namespace BankSphere.Accounts.Domain;

// Separate from the account aggregate: only a future authenticated ledger/simulator projection adapter
// may populate this table. No Account repository or public endpoint writes balance projections.
public sealed record AccountBalanceProjection(Guid AccountId, AccountBalanceSnapshot Snapshot)
{
    private AccountBalanceProjection() : this(Guid.Empty, null!) { }
}
