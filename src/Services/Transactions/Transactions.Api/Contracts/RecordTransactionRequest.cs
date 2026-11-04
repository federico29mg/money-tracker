using Transactions.Domain.Common;

namespace Transactions.Api.Contracts;

// CategoryId and CurrencyCode are optional: leave them out to use the default category and the account's currency.
public sealed record RecordTransactionRequest(
    Guid AccountId,
    Guid? CategoryId,
    TransactionType Type,
    decimal Amount,
    string? CurrencyCode,
    DateOnly Date,
    string Name,
    string? Description);