using BankSphere.SharedKernel;

namespace BankSphere.Transactions.Application;

public sealed class GetTransactionByIdHandler(ITransactionRepository repository)
{
    public async Task<TransactionDetails> Handle(Guid id, CancellationToken cancellationToken) =>
        await repository.FindAsync(id, cancellationToken)
        ?? throw new BusinessException("transaction.not_found", "Transaction does not exist.", FailureKind.NotFound);
}
