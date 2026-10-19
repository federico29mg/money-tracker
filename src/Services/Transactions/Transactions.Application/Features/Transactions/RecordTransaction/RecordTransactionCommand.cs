using Transactions.Domain.Common;

namespace Transactions.Application.Features.Transactions.RecordTransaction;

// A null CurrencyCode means the account's currency.
public sealed record RecordTransactionCommand(
    Guid UserId,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    string? CurrencyCode,
    DateOnly Date,
    string Name,
    string? Description);