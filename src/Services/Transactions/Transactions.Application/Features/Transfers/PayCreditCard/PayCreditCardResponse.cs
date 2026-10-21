using Transactions.Application.Features.Transactions.SettleTransactions;

namespace Transactions.Application.Features.Transfers.PayCreditCard;

public sealed record PayCreditCardResponse(Guid TransferId, SettlementResponse? Settlement);