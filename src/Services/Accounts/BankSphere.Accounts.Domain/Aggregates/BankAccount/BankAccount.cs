using BankSphere.SharedKernel;

namespace BankSphere.Accounts.Domain;

public sealed class BankAccount : BankingAggregate
{
    private readonly List<AccountOwnership> ownerships = [];
    private readonly List<AccountMandate> mandates = [];
    private readonly List<AccountRestriction> restrictions = [];
    private readonly List<AccountStatusHistory> statusHistory = [];
    private BankAccount() { }
    public string AccountNumber { get; private set; } = "";
    public Iban? Iban { get; private set; }
    public string? Bic { get; private set; }
    public AccountType Type { get; private set; }
    public AccountStatus Status { get; private set; }
    public string ProductCode { get; private set; } = "";
    public string ProductName { get; private set; } = "";
    public Currency Currency { get; private set; } = null!;
    public Guid PrimaryOwnerId { get; private set; }
    public string BranchId { get; private set; } = "";
    public DateTimeOffset OpeningDateUtc { get; private set; }
    public DateTimeOffset? ActivationDateUtc { get; private set; }
    public DateTimeOffset? ClosingDateUtc { get; private set; }
    public string Purpose { get; private set; } = "";
    public string? OverdraftFacilityReference { get; private set; }
    public string StatementFrequency { get; private set; } = "Monthly";
    public string StatementDeliveryMethod { get; private set; } = "SecurePortal";
    public string? InterestRatePlanReference { get; private set; }
    public string? FeePlanReference { get; private set; }
    public Money DailyTransactionLimit { get; private set; } = null!;
    public Money MonthlyTransactionLimit { get; private set; } = null!;
    // No setter/method accepts a balance. Snapshots are a separate read projection repository.
    public IReadOnlyCollection<AccountOwnership> Ownerships => ownerships.AsReadOnly();
    public IReadOnlyCollection<AccountMandate> Mandates => mandates.AsReadOnly();
    public IReadOnlyCollection<AccountRestriction> Restrictions => restrictions.AsReadOnly();
    public IReadOnlyCollection<AccountStatusHistory> StatusHistory => statusHistory.AsReadOnly();

    public static BankAccount OpenAccount(Guid owner, AccountType type, string productCode, string productName, Currency currency,
        string branch, string purpose, Money dailyLimit, Money monthlyLimit, string actor, DateTimeOffset now)
    {
        Guard.Against(dailyLimit.Currency != currency || monthlyLimit.Currency != currency || dailyLimit.Amount > monthlyLimit.Amount,
            "account.limits", "Account limits must share the account currency and daily must not exceed monthly.");
        var account = new BankAccount { PrimaryOwnerId = Guard.Id(owner, "Primary owner"), Type = Guard.Defined(type),
            ProductCode = Guard.Text(productCode, "Product code"), ProductName = Guard.Text(productName, "Product name"), Currency = currency,
            BranchId = Guard.Text(branch, "Branch"), Purpose = Guard.Text(purpose, "Purpose"), DailyTransactionLimit = dailyLimit,
            MonthlyTransactionLimit = monthlyLimit, OpeningDateUtc = Guard.Utc(now) };
        account.AccountNumber = $"ACC-{account.Id:N}"; account.Initialize(now);
        account.ownerships.Add(new(Guid.NewGuid(), owner, OwnershipType.Primary, now, null));
        account.statusHistory.Add(new(Guid.NewGuid(), AccountStatus.Pending, AccountStatus.Pending, "Account opened pending eligibility", Guard.Text(actor, "Actor"), now));
        return account;
    }

    private void Transition(AccountStatus next, string reason, string actor, DateTimeOffset now, params AccountStatus[] allowed)
    {
        Guard.Against(!allowed.Contains(Status), "account.transition", $"Cannot change {Status} to {next}.", FailureKind.Conflict);
        statusHistory.Add(new(Guid.NewGuid(), Status, next, Guard.Text(reason, "Reason"), Guard.Text(actor, "Actor"), Guard.Utc(now)));
        Status = next; Changed(now);
    }

    public void ActivateAccount(AccountEligibility eligibility, string actor, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(eligibility);
        Guard.Utc(eligibility.CheckedAtUtc);
        Guard.Text(eligibility.EvidenceReference, "Eligibility evidence");
        Guard.Against(!eligibility.CustomerEligible || !eligibility.ComplianceEligible || eligibility.CheckedAtUtc > now || now - eligibility.CheckedAtUtc > TimeSpan.FromMinutes(5),
            "account.eligibility", "Recent customer and compliance eligibility checks are required.", FailureKind.Conflict);
        Guard.Against(restrictions.Any(item => item.RemovedAtUtc is null && item.EffectiveAtUtc <= now && (item.ExpiresAtUtc is null || item.ExpiresAtUtc > now)), "account.restricted", "Remove active restrictions before activation.", FailureKind.Conflict);
        Transition(AccountStatus.Active, "Eligibility verified", actor, now, AccountStatus.Pending, AccountStatus.Suspended);
        ActivationDateUtc ??= now;
    }
    public void SuspendAccount(string reason, string actor, DateTimeOffset now) => Transition(AccountStatus.Suspended, reason, actor, now, AccountStatus.Active);
    public void FreezeAccount(string reason, string actor, DateTimeOffset now) => Transition(AccountStatus.Frozen, reason, actor, now, AccountStatus.Active, AccountStatus.Suspended);
    public void UnfreezeAccount(string reason, string actor, DateTimeOffset now) => Transition(AccountStatus.Suspended, reason, actor, now, AccountStatus.Frozen);
    public void CloseAccount(AccountBalanceSnapshot snapshot, string reason, string actor, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Guard.Against(snapshot.Source != BalanceSource.AuthoritativeLedger || snapshot.BookedBalance.Currency != Currency
            || snapshot.BookedBalance.Amount != 0 || snapshot.AvailableBalance.Amount != 0 || snapshot.PendingAmount.Amount != 0
            || snapshot.AsOfUtc > now || now - snapshot.AsOfUtc > TimeSpan.FromMinutes(5), "account.closure_balance", "Closure requires a recent authoritative zero-balance projection without pending amounts.", FailureKind.Conflict);
        Transition(AccountStatus.Closed, reason, actor, now, AccountStatus.Active, AccountStatus.Suspended, AccountStatus.Pending); ClosingDateUtc = now;
    }
    private void Editable() => Guard.Against(Status == AccountStatus.Closed, "account.closed", "Closed accounts cannot change.", FailureKind.Conflict);
    public void AssignBankIdentifiers(Iban iban, string bic, DateTimeOffset now)
    {
        Editable();
        Guard.Against(Iban is not null, "account.identifiers_assigned", "Bank identifiers are already assigned.", FailureKind.Conflict);
        Guard.Against(bic.Length is not (8 or 11) || bic.Any(character => !char.IsAsciiLetterOrDigit(character)), "account.bic", "Invalid BIC.");
        Iban = iban; Bic = bic.ToUpperInvariant(); Changed(now);
    }
    public void ConfigureServicing(string statementFrequency, string deliveryMethod, string? interestPlan,
        string? feePlan, string? overdraftReference, DateTimeOffset now)
    {
        Editable();
        StatementFrequency = Guard.Text(statementFrequency, "Statement frequency");
        StatementDeliveryMethod = Guard.Text(deliveryMethod, "Delivery method");
        InterestRatePlanReference = interestPlan is null ? null : Guard.Text(interestPlan, "Interest plan");
        FeePlanReference = feePlan is null ? null : Guard.Text(feePlan, "Fee plan");
        OverdraftFacilityReference = overdraftReference is null ? null : Guard.Text(overdraftReference, "Facility reference"); Changed(now);
    }
    public void AddAuthorizedOwner(Guid customerId, OwnershipType type, DateTimeOffset now)
    {
        Editable();
        Guard.Defined(type);
        Guard.Against(type == OwnershipType.Primary || ownerships.Any(item => item.CustomerId == customerId && item.EndDateUtc is null), "account.owner", "Primary owner cannot be replaced or active ownership duplicated.", FailureKind.Conflict);
        ownerships.Add(new(Guid.NewGuid(), Guard.Id(customerId, "Customer"), type, Guard.Utc(now), null)); Changed(now);
    }
    public void AddMandate(Guid person, string type, string permissions, DateTimeOffset expiry, DateTimeOffset now)
    {
        Editable();
        Guard.Against(expiry <= now, "account.mandate_expiry", "Mandate must expire in the future.");
        mandates.Add(new(Guid.NewGuid(), Guard.Id(person, "Authorized person"), Guard.Text(type, "Mandate"), Guard.Text(permissions, "Permissions"), Guard.Utc(expiry), null)); Changed(now);
    }
    public void RevokeMandate(Guid id, DateTimeOffset now)
    {
        Editable();
        var index = mandates.FindIndex(item => item.Id == id);
        Guard.Against(index < 0, "account.mandate_not_found", "Mandate does not exist.", FailureKind.NotFound);
        Guard.Against(mandates[index].RevokedAtUtc is not null, "account.mandate_revoked", "Mandate is already revoked.", FailureKind.Conflict);
        mandates[index] = mandates[index] with { RevokedAtUtc = Guard.Utc(now) }; Changed(now);
    }
    public void AddRestriction(string type, string reason, DateTimeOffset? expiry, DateTimeOffset now)
    {
        Editable();
        Guard.Against(expiry <= now, "account.restriction_expiry", "Expiry must be in the future.");
        if (expiry is not null) Guard.Utc(expiry.Value);
        restrictions.Add(new(Guid.NewGuid(), Guard.Text(type, "Restriction"), Guard.Text(reason, "Reason"), Guard.Utc(now), expiry, null)); Changed(now);
    }
    public void RemoveRestriction(Guid id, DateTimeOffset now)
    {
        Editable();
        var index = restrictions.FindIndex(item => item.Id == id);
        Guard.Against(index < 0, "account.restriction_not_found", "Restriction does not exist.", FailureKind.NotFound);
        Guard.Against(restrictions[index].RemovedAtUtc is not null, "account.restriction_removed", "Restriction already removed.", FailureKind.Conflict);
        restrictions[index] = restrictions[index] with { RemovedAtUtc = Guard.Utc(now) }; Changed(now);
    }
}
