using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;

namespace Transactions.Application.Features.Accounts;

// Balance: opening balance plus settled transactions plus transfers in minus transfers out, in Currency.
// Pending: unsettled foreign-currency transactions per currency (negative when expenses outweigh income).
public sealed record AccountResponse(
    Guid Id,
    string Name,
    AccountType Type,
    string Currency,
    decimal OpeningBalance,
    decimal Balance,
    IReadOnlyList<PendingAmountResponse> Pending,
    bool IsArchived)
{
    public static AccountResponse From(Account account, AccountTotals totals, decimal transfersNet)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(totals);

        return new AccountResponse(
            account.Id.Value,
            account.Name,
            account.Type,
            account.Currency.Code,
            account.OpeningBalance.Amount,
            account.OpeningBalance.Amount + totals.SettledNet + transfersNet,
            [.. totals.PendingNet.Select(money => new PendingAmountResponse(money.Currency.Code, money.Amount))],
            account.IsArchived);
    }
}