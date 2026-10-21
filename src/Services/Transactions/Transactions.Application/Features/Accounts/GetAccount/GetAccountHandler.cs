using Transactions.Application.Abstractions;

namespace Transactions.Application.Features.Accounts.GetAccount;

public sealed class GetAccountHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    ITransferRepository transferRepository)
{
    public async Task<AccountResponse> HandleAsync(GetAccountQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var account = await accountRepository.GetRequiredAsync(query.AccountId, query.UserId, cancellationToken);

        var totals = await transactionRepository.GetTotalsAsync([account.Id], account.UserId, cancellationToken);
        var transfers =
            await transferRepository.GetNetByAccountAsync([account.Id], account.UserId, cancellationToken);

        return AccountResponse.From(
            account,
            totals.SingleOrDefault() ?? AccountTotals.Empty(account.Id),
            transfers.GetValueOrDefault(account.Id));
    }
}