using Transactions.Domain.AccountAggregate;

namespace Transactions.Application.Features.Accounts.OpenAccount;

public sealed record OpenAccountCommand(
    Guid UserId,
    string Name,
    AccountType Type,
    string CurrencyCode,
    decimal OpeningBalance);