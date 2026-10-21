using Transactions.Application.Abstractions;
using Transactions.Application.Features.Accounts;

namespace Transactions.Application.Features.Transactions.SettleTransactions;

public sealed class SettleTransactionsHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
{
    private readonly TransactionSettler _settler = new(transactionRepository);

    public async Task<SettlementResponse> HandleAsync(
        SettleTransactionsCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var account = await accountRepository.GetRequiredAsync(command.AccountId, command.UserId, cancellationToken);

        var settlement = await _settler.SettleAsync(
            account, command.TransactionIds, command.ExchangeRate, command.TotalPaid, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return settlement;
    }
}