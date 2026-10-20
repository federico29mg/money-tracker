namespace Transactions.Api.Contracts;

// Send either ExchangeRate (account currency per 1 unit, e.g. 4103.25) or TotalPaid (in the account's currency).
public sealed record SettleTransactionsRequest(
    IReadOnlyList<Guid> TransactionIds,
    decimal? ExchangeRate,
    decimal? TotalPaid);