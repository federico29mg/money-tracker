namespace Transactions.Application.Features.Transactions.SettleTransactions;

// Exactly one of ExchangeRate or TotalPaid. TotalPaid is in the account's currency and is turned into a rate.
// All transactions must be on the account and in the same foreign currency.
public sealed record SettleTransactionsCommand(
    Guid UserId,
    Guid AccountId,
    IReadOnlyList<Guid> TransactionIds,
    decimal? ExchangeRate,
    decimal? TotalPaid);