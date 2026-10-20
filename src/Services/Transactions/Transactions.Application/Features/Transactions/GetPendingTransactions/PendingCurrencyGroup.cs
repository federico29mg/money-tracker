namespace Transactions.Application.Features.Transactions.GetPendingTransactions;

// Total: income minus expenses in Currency, so a group of charges is negative.
public sealed record PendingCurrencyGroup(
    string Currency,
    decimal Total,
    IReadOnlyList<TransactionResponse> Transactions);