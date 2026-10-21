namespace Transactions.Api.Contracts;

// Amount is in the paying account's currency; ReceivedAmount, in the card's, only when they differ.
// Settlement is optional: the foreign-currency charges this payment covered.
public sealed record PayCreditCardRequest(
    Guid FromAccountId,
    decimal Amount,
    decimal? ReceivedAmount,
    DateOnly Date,
    string? Description,
    SettleTransactionsRequest? Settlement);