namespace Transactions.Application.Features.Transfers.RecordTransfer;

// Amount is in the source account's currency; ReceivedAmount, in the destination's, is only needed when they differ.
public sealed record RecordTransferCommand(
    Guid UserId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    decimal? ReceivedAmount,
    DateOnly Date,
    string? Description);