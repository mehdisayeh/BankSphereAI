using BankSphere.Customers.Domain;
using BankSphere.SharedKernel;

namespace BankSphere.Customers.Application;

public sealed class CreateCustomerHandler(ICustomerRepository repository, TimeProvider clock)
{
    public async Task<CustomerDetails> Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Residence); ArgumentNullException.ThrowIfNull(command.Financial);
        Guard.Against(command.TaxResidencyCountries is null || command.TaxResidencyCountries.Count == 0, "customer.tax_residency", "At least one tax residency country is required.");
        var now = clock.GetUtcNow();
        var customer = Customer.RegisterCustomer(command.Type, command.Segment, command.Individual, command.Corporate, command.Financial,
            command.OnboardingChannel, command.PreferredLanguage, command.PreferredCommunicationChannel, now);
        customer.UpdateAddress("residential", command.Residence, now);
        customer.UpdateContactInformation("email", command.Email, now);
        if (!string.IsNullOrWhiteSpace(command.Phone)) customer.UpdateContactInformation("phone", command.Phone, now);
        foreach (var country in command.TaxResidencyCountries!) customer.AddCountry("tax", country, now);
        foreach (var country in command.AdditionalNationalities ?? []) customer.AddCountry("nationality", country, now);
        await repository.AddAsync(customer, cancellationToken); return CustomerDetails.From(customer);
    }
}
