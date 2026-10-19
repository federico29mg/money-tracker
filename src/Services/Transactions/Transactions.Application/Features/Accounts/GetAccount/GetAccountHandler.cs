using Transactions.Application.Abstractions;

namespace Transactions.Application.Features.Accounts.GetAccount;

public sealed class GetAccountHandler(IAccountRepository accountRepository)
{
    public async Task<AccountResponse> HandleAsync(GetAccountQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var account = await accountRepository.GetRequiredAsync(query.AccountId, query.UserId, cancellationToken);

        return AccountResponse.From(account);
    }
}