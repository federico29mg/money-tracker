using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Abstractions;

public sealed record AccountTotals(AccountId AccountId, decimal SettledNet, IReadOnlyList<Money> PendingNet)
{
    public static AccountTotals Empty(AccountId accountId) => new(accountId, 0m, []);
}