using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Accounts;

internal static class AccountRepositoryExtensions
{
    public static async Task<Account> GetRequiredAsync(
        this IAccountRepository repository,
        Guid accountId,
        Guid userId,
        CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(AccountId.From(accountId), UserId.From(userId), cancellationToken)
        ?? throw new NotFoundException($"Account '{accountId}' was not found.");
}