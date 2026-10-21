using Transactions.Application.Abstractions;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Accounts.GetAccounts;

public sealed class GetAccountsHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    ITransferRepository transferRepository)
{
    public async Task<IReadOnlyList<AccountResponse>> HandleAsync(
        GetAccountsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var userId = UserId.From(query.UserId);
        var accounts = await accountRepository.ListAsync(userId, query.IncludeArchived, cancellationToken);
        var accountIds = accounts.Select(account => account.Id).ToList();

        // One query per kind of total for all accounts, instead of queries per account.
        var totals = (await transactionRepository.GetTotalsAsync(accountIds, userId, cancellationToken))
            .ToDictionary(accountTotals => accountTotals.AccountId);
        var transfers = await transferRepository.GetNetByAccountAsync(accountIds, userId, cancellationToken);

        return
        [
            .. accounts.Select(account => AccountResponse.From(
                account,
                totals.GetValueOrDefault(account.Id) ?? AccountTotals.Empty(account.Id),
                transfers.GetValueOrDefault(account.Id)))
        ];
    }
}