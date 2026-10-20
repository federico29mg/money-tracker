namespace Transactions.Application.Features.Transactions.SettleTransactions;

// TotalSettled can differ from a TotalPaid by a few cents, because each transaction is rounded on its own.
public sealed record SettlementResponse(
    int TransactionCount,
    string Currency,
    decimal ExchangeRate,
    string SettledCurrency,
    decimal TotalSettled);