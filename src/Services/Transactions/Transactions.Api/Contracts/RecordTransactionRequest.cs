using Transactions.Domain.Common;

namespace Transactions.Api.Contracts;

// CurrencyCode is optional: leave it out to use the account's currency.
public sealed record RecordTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    string? CurrencyCode,
    DateOnly Date,
    string Name,
    string? Description);