
namespace BankSphere.Customers.Domain;

public sealed record CorporateProfile(string RegisteredBusinessName, string RegistrationNumber,
    DateOnly IncorporationDate, string Industry, string LegalForm, string RegistrationCountry);
