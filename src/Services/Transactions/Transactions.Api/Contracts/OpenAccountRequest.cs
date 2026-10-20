using Transactions.Domain.AccountAggregate;

namespace Transactions.Api.Contracts;

public sealed record OpenAccountRequest(string Name, AccountType Type, string CurrencyCode, decimal OpeningBalance);