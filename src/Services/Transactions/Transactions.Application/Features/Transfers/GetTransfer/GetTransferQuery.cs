namespace Transactions.Application.Features.Transfers.GetTransfer;

public sealed record GetTransferQuery(Guid UserId, Guid TransferId);