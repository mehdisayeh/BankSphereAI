using BankSphere.Customers.Domain;
using BankSphere.SharedKernel;

namespace BankSphere.Customers.Application;

public sealed record CreateCustomer(CustomerType Type, CustomerSegment Segment, IndividualProfile? Individual,
    CorporateProfile? Corporate, FinancialProfile Financial, Address Residence, string Email, string? Phone,
    string OnboardingChannel, string PreferredLanguage, string PreferredCommunicationChannel,
    IReadOnlyList<string> TaxResidencyCountries, IReadOnlyList<string>? AdditionalNationalities);
