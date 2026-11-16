
namespace BankSphere.CreditAssessment.Application;

public interface ICreditCustomerGateway { Task<bool> ExistsAsync(Guid customer, CancellationToken cancellationToken); }
