
namespace BankSphere.Customers.Domain;

public sealed record IndividualProfile(string LegalFirstName, string LegalLastName, string? PreferredName,
    DateOnly DateOfBirth, string PlaceOfBirth, string CountryOfBirth, string Nationality);
