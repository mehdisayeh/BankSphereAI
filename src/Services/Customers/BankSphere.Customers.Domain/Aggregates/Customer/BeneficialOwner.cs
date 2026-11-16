
namespace BankSphere.Customers.Domain;

public sealed record BeneficialOwner(Guid Id, Guid OwnerReference, decimal OwnershipPercentage, string ControlType);
