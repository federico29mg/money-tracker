using Transactions.Domain.AccountAggregate;

namespace Transactions.Application.Features.Accounts;

public sealed record AccountResponse(
    Guid Id,
    string Name,
    AccountType Type,
    string Currency,
    decimal OpeningBalance,
    bool IsArchived)
{
    public static AccountResponse From(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new AccountResponse(
            account.Id.Value,
            account.Name,
            account.Type,
            account.Currency.Code,
            account.OpeningBalance.Amount,
            account.IsArchived);
    }
}