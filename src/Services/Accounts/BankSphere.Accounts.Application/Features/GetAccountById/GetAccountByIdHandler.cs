using BankSphere.SharedKernel;

namespace BankSphere.Accounts.Application;

public sealed class GetAccountByIdHandler(IAccountRepository repository)
{
    public async Task<AccountDetails> Handle(Guid id, CancellationToken cancellationToken) => AccountDetails.From(
        await repository.FindAsync(Guard.Id(id, "Account"), cancellationToken) ?? throw new BusinessException("account.not_found", "Account does not exist.", FailureKind.NotFound));
}
