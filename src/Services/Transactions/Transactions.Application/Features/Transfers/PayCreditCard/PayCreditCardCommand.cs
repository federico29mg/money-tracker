namespace Transactions.Application.Features.Transfers.PayCreditCard;

// A transfer from FromAccountId to the card, optionally settling foreign-currency charges in the same save.
public sealed record PayCreditCardCommand(
    Guid UserId,
    Guid CardAccountId,
    Guid FromAccountId,
    decimal Amount,
    decimal? ReceivedAmount,
    DateOnly Date,
    string? Description,
    PaymentSettlement? Settlement);