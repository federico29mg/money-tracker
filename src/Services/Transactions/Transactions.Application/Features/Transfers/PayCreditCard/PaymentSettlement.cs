namespace Transactions.Application.Features.Transfers.PayCreditCard;

// The card's foreign-currency charges this payment covered, with the bank's rate or what was paid for them.
public sealed record PaymentSettlement(IReadOnlyList<Guid> TransactionIds, decimal? ExchangeRate, decimal? TotalPaid);