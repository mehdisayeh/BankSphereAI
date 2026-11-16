using BankSphere.Customers.Domain;

namespace BankSphere.Customers.Application;

public sealed record CustomerDetails(Guid Id, string CustomerNumber, CustomerType Type, CustomerStatus Status, CustomerSegment Segment,
    IndividualProfile? Individual, CorporateProfile? Corporate, FinancialProfile Financial, KycStatus KycStatus,
    string OnboardingChannel, string Language, string CommunicationChannel, DateTimeOffset CreatedAtUtc, long Version,
    IReadOnlyList<ContactDetails> Contacts, IReadOnlyList<AddressDetails> Addresses, IReadOnlyList<DocumentDetails> Documents,
    IReadOnlyList<string> TaxResidencyCountries, IReadOnlyList<string> AdditionalNationalities)
{
    public static CustomerDetails From(Customer customer) => new(customer.Id, customer.CustomerNumber, customer.Type, customer.Status, customer.Segment,
        customer.Individual, customer.Corporate, customer.Financial, customer.KycStatus, customer.OnboardingChannel, customer.PreferredLanguage,
        customer.PreferredCommunicationChannel, customer.CreatedAtUtc, customer.Version,
        customer.Contacts.Select(contact => new ContactDetails(contact.Type, contact.Value, contact.Verified)).ToArray(),
        customer.Addresses.Select(address => new AddressDetails(address.Type, address.Street, address.City, address.Region, address.PostalCode, address.Country, address.EffectiveFromUtc, address.EffectiveToUtc)).ToArray(),
        customer.IdentityDocuments.Select(document => new DocumentDetails(document.Id, document.DocumentType, document.IssuingCountry, document.IssueDate, document.ExpiryDate, document.Verified)).ToArray(),
        customer.Countries.Where(item => item.Purpose == "tax").Select(item => item.CountryCode).ToArray(),
        customer.Countries.Where(item => item.Purpose == "nationality").Select(item => item.CountryCode).ToArray());
}
