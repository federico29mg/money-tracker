namespace Transactions.Api.Contracts;

// Amount is in the source account's currency. ReceivedAmount, in the destination's, is only needed when they differ.
public sealed record RecordTransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    decimal? ReceivedAmount,
    DateOnly Date,
    string? Description);