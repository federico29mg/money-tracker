using Transactions.Application.Abstractions;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Accounts.GetAccounts;

public sealed class GetAccountsHandler(IAccountRepository accountRepository)
{
    public async Task<IReadOnlyList<AccountResponse>> HandleAsync(
        GetAccountsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var accounts = await accountRepository.ListAsync(
            UserId.From(query.UserId), query.IncludeArchived, cancellationToken);

        return accounts.Select(AccountResponse.From).ToList();
    }
}