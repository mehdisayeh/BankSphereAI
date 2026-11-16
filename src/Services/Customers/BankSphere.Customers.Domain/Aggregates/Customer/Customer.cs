using BankSphere.SharedKernel;

namespace BankSphere.Customers.Domain;

public sealed class Customer : BankingAggregate
{
    private readonly List<CustomerAddress> addresses = [];
    private readonly List<CustomerContact> contacts = [];
    private readonly List<IdentityDocument> identityDocuments = [];
    private readonly List<CustomerConsent> consents = [];
    private readonly List<CustomerRelationship> relationships = [];
    private readonly List<BeneficialOwner> beneficialOwners = [];
    private readonly List<CustomerCountry> countries = [];
    private readonly List<CustomerRiskProfile> riskProfiles = [];
    private Customer() { }

    public string CustomerNumber { get; private set; } = "";
    public CustomerType Type { get; private set; }
    public CustomerSegment Segment { get; private set; }
    public CustomerStatus Status { get; private set; }
    public IndividualProfile? Individual { get; private set; }
    public CorporateProfile? Corporate { get; private set; }
    public FinancialProfile Financial { get; private set; } = null!;
    public DateTimeOffset CustomerSince { get; private set; }
    public string OnboardingChannel { get; private set; } = "";
    public string PreferredLanguage { get; private set; } = "";
    public string PreferredCommunicationChannel { get; private set; } = "";
    public Guid? RelationshipManagerId { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public DateTimeOffset? KycLastReviewedAtUtc { get; private set; }
    public DateTimeOffset? KycNextReviewAtUtc { get; private set; }
    public IReadOnlyCollection<CustomerAddress> Addresses => addresses.AsReadOnly();
    public IReadOnlyCollection<CustomerContact> Contacts => contacts.AsReadOnly();
    public IReadOnlyCollection<IdentityDocument> IdentityDocuments => identityDocuments.AsReadOnly();
    public IReadOnlyCollection<CustomerConsent> Consents => consents.AsReadOnly();
    public IReadOnlyCollection<CustomerRelationship> Relationships => relationships.AsReadOnly();
    public IReadOnlyCollection<BeneficialOwner> BeneficialOwners => beneficialOwners.AsReadOnly();
    public IReadOnlyCollection<CustomerCountry> Countries => countries.AsReadOnly();
    public IReadOnlyCollection<CustomerRiskProfile> RiskProfiles => riskProfiles.AsReadOnly();

    public static Customer RegisterCustomer(CustomerType type, CustomerSegment segment, IndividualProfile? individual,
        CorporateProfile? corporate, FinancialProfile financial, string channel, string language, string communication, DateTimeOffset now)
    {
        Guard.Utc(now);
        Guard.Defined(type);
        Guard.Against(type == CustomerType.Individual ? individual is null || corporate is not null : corporate is null || individual is not null,
            "customer.identity", "Supply an individual profile OR a corporate profile matching customer type.");
        if (individual is not null)
        {
            var today = DateOnly.FromDateTime(now.UtcDateTime);
            Guard.Against(individual.DateOfBirth > today.AddYears(-18) || individual.DateOfBirth < today.AddYears(-120), "customer.age", "Individual onboarding requires age 18..120.");
            individual = individual with { LegalFirstName = Guard.Text(individual.LegalFirstName, "First name"), LegalLastName = Guard.Text(individual.LegalLastName, "Last name"),
                PlaceOfBirth = Guard.Text(individual.PlaceOfBirth, "Birth place"), CountryOfBirth = Guard.Country(individual.CountryOfBirth), Nationality = Guard.Country(individual.Nationality) };
        }
        if (corporate is not null)
        {
            Guard.Against(corporate.IncorporationDate > DateOnly.FromDateTime(now.UtcDateTime) || corporate.IncorporationDate == default, "customer.incorporation", "Invalid incorporation date.");
            corporate = corporate with { RegisteredBusinessName = Guard.Text(corporate.RegisteredBusinessName, "Business name"), RegistrationNumber = Guard.Text(corporate.RegistrationNumber, "Registration"),
                Industry = Guard.Text(corporate.Industry, "Industry"), LegalForm = Guard.Text(corporate.LegalForm, "Legal form"), RegistrationCountry = Guard.Country(corporate.RegistrationCountry) };
        }
        ArgumentNullException.ThrowIfNull(financial);
        var customer = new Customer { Type = type, Segment = Guard.Defined(segment), Individual = individual, Corporate = corporate,
            Financial = financial with { Occupation = Guard.Text(financial.Occupation, "Occupation"), EmploymentStatus = Guard.Defined(financial.EmploymentStatus),
                AnnualIncomeRange = Guard.Text(financial.AnnualIncomeRange, "Income range"), SourceOfFunds = Guard.Text(financial.SourceOfFunds, "Source of funds"),
                SourceOfWealth = Guard.Text(financial.SourceOfWealth, "Source of wealth"), ExpectedAccountActivity = Guard.Text(financial.ExpectedAccountActivity, "Expected activity") },
            OnboardingChannel = Guard.Text(channel, "Channel"), PreferredLanguage = Guard.Text(language, "Language", 10),
            PreferredCommunicationChannel = Guard.Text(communication, "Communication channel"), CustomerSince = now };
        customer.CustomerNumber = $"CUS-{customer.Id:N}";
        customer.Initialize(now);
        customer.RaiseDomainEvent(new CustomerCreated(customer.Id, Guid.NewGuid(), now));
        return customer;
    }

    private void Editable() => Guard.Against(Status == CustomerStatus.Closed, "customer.closed", "Closed customer relationships cannot change.", FailureKind.Conflict);

    public void UpdateContactInformation(string type, string value, DateTimeOffset now)
    {
        Editable();
        Guard.Against(type is not ("email" or "phone"), "customer.contact_type", "Contact type must be email or phone.");
        value = type == "email" ? new EmailAddress(value).Value : Guard.Text(value, "Phone", 30);
        contacts.RemoveAll(contact => contact.Type == type);
        contacts.Add(new(Guid.NewGuid(), type, value, false));
        Changed(now);
    }

    public void UpdateAddress(string type, Address address, DateTimeOffset now)
    {
        Editable();
        var validated = address.Validate();
        type = Guard.Text(type, "Address type");
        for (var index = 0; index < addresses.Count; index++)
            if (addresses[index].Type == type && addresses[index].EffectiveToUtc is null) addresses[index] = addresses[index] with { EffectiveToUtc = Guard.Utc(now) };
        addresses.Add(new(Guid.NewGuid(), type, validated.Street, validated.City, validated.Region, validated.PostalCode, validated.Country, Guard.Utc(now), null));
        Changed(now);
    }

    public void AddCountry(string purpose, string country, DateTimeOffset now)
    {
        Editable();
        Guard.Against(purpose is not ("nationality" or "tax"), "customer.country_purpose", "Country purpose must be nationality or tax.");
        country = Guard.Country(country);
        Guard.Against(countries.Any(item => item.Purpose == purpose && item.CountryCode == country), "customer.duplicate_country", "Country already recorded.", FailureKind.Conflict);
        countries.Add(new(Guid.NewGuid(), purpose, country)); Changed(now);
    }

    public void AddIdentityDocument(string type, string country, DateOnly issue, DateOnly expiry, string reference, DateTimeOffset now)
    {
        Editable();
        Guard.Against(issue > DateOnly.FromDateTime(now.UtcDateTime) || expiry <= issue || expiry <= DateOnly.FromDateTime(now.UtcDateTime), "customer.document_dates", "Identity document must be issued and unexpired.");
        identityDocuments.Add(new(Guid.NewGuid(), Guard.Text(type, "Document type"), Guard.Country(country), issue, expiry, Guard.Text(reference, "Protected document reference"), false)); Changed(now);
    }

    public void VerifyIdentity(Guid documentId, DateTimeOffset nextReview, DateTimeOffset now)
    {
        Editable();
        var index = identityDocuments.FindIndex(document => document.Id == documentId);
        Guard.Against(index < 0, "customer.document_not_found", "Identity document does not exist.", FailureKind.NotFound);
        Guard.Against(identityDocuments[index].ExpiryDate <= DateOnly.FromDateTime(now.UtcDateTime) || nextReview <= now, "customer.kyc_dates", "Document must be unexpired and next review in the future.");
        identityDocuments[index] = identityDocuments[index] with { Verified = true };
        KycStatus = KycStatus.Verified; KycLastReviewedAtUtc = Guard.Utc(now); KycNextReviewAtUtc = Guard.Utc(nextReview); Changed(now);
    }

    public void ChangeCustomerStatus(CustomerStatus status, DateTimeOffset now)
    {
        Editable(); Guard.Defined(status);
        Guard.Against(status == Status || status == CustomerStatus.Onboarding, "customer.transition", "Invalid customer transition.", FailureKind.Conflict);
        Guard.Against(status == CustomerStatus.Active && (KycStatus != KycStatus.Verified || KycNextReviewAtUtc <= now), "customer.kyc_required", "Current KYC is required before activation.", FailureKind.Conflict);
        Status = status; Changed(now);
    }
    public void SuspendCustomer(DateTimeOffset now) => ChangeCustomerStatus(CustomerStatus.Suspended, now);
    public void AssignRelationshipManager(Guid managerId, DateTimeOffset now)
    {
        Editable(); RelationshipManagerId = Guard.Id(managerId, "Relationship manager"); Changed(now);
    }
    public void CloseCustomerRelationship(DateTimeOffset now) => ChangeCustomerStatus(CustomerStatus.Closed, now);
    public void UpdateRiskProfile(CustomerRiskProfile profile, DateTimeOffset now) { Editable(); ArgumentNullException.ThrowIfNull(profile); riskProfiles.Add(profile); Changed(now); }
    public void RecordConsent(string type, string policyVersion, DateTimeOffset now)
    {
        Editable(); consents.Add(new(Guid.NewGuid(), Guard.Text(type, "Consent type"), Guard.Text(policyVersion, "Policy version"), Guard.Utc(now), null)); Changed(now);
    }
    public void WithdrawConsent(Guid consentId, DateTimeOffset now)
    {
        Editable(); var index = consents.FindIndex(item => item.Id == consentId);
        Guard.Against(index < 0, "customer.consent_not_found", "Consent does not exist.", FailureKind.NotFound);
        Guard.Against(consents[index].WithdrawnAtUtc is not null, "customer.consent_withdrawn", "Consent already withdrawn.", FailureKind.Conflict);
        consents[index] = consents[index] with { WithdrawnAtUtc = Guard.Utc(now) }; Changed(now);
    }
    public void AddRelationship(Guid relatedId, string type, DateTimeOffset now)
    {
        Editable(); Guard.Against(relatedId == Id, "customer.self_relationship", "Cannot relate a customer to itself.");
        relationships.Add(new(Guid.NewGuid(), Guard.Id(relatedId, "Related customer"), Guard.Text(type, "Relationship"), Guard.Utc(now), null)); Changed(now);
    }
    public void AddBeneficialOwner(Guid reference, Percentage ownership, string control, DateTimeOffset now)
    {
        Editable(); Guard.Against(Type != CustomerType.LegalEntity || beneficialOwners.Sum(owner => owner.OwnershipPercentage) + ownership.Value > 100, "customer.ownership", "Beneficial ownership requires a corporation and total ownership no greater than 100%.");
        Guard.Against(beneficialOwners.Any(owner => owner.OwnerReference == reference), "customer.owner_duplicate", "Owner already recorded.", FailureKind.Conflict);
        beneficialOwners.Add(new(Guid.NewGuid(), Guard.Id(reference, "Owner"), ownership.Value, Guard.Text(control, "Control type"))); Changed(now);
    }
}
