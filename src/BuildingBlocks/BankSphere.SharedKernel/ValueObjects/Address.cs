
namespace BankSphere.SharedKernel;

public sealed record Address(string Street, string City, string? Region, string PostalCode, string Country)
{
    public Address Validate() => new(Guard.Text(Street, "Street"), Guard.Text(City, "City"), Region,
        Guard.Text(PostalCode, "Postal code", 20), Guard.Country(Country));
}
