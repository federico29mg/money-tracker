namespace Transactions.Application.Features.Transactions.GetPendingTransactions;

public sealed record PendingTransactionsResponse(
    Guid AccountId,
    string AccountCurrency,
    IReadOnlyList<PendingCurrencyGroup> Groups);