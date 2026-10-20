using Transactions.Application.Abstractions;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Accounts.GetAccounts;

public sealed class GetAccountsHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
{
    public async Task<IReadOnlyList<AccountResponse>> HandleAsync(
        GetAccountsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var userId = UserId.From(query.UserId);
        var accounts = await accountRepository.ListAsync(userId, query.IncludeArchived, cancellationToken);

        // One query for all accounts instead of one per account.
        var totals = (await transactionRepository.GetTotalsAsync(
                [.. accounts.Select(account => account.Id)], userId, cancellationToken))
            .ToDictionary(accountTotals => accountTotals.AccountId);

        return [.. accounts
            .Select(account => AccountResponse.From(
                account, totals.GetValueOrDefault(account.Id) ?? AccountTotals.Empty(account.Id)))];
    }
}