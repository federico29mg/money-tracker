using Transactions.Domain.TransferAggregate;

namespace Transactions.Application.Features.Transfers;

public sealed record TransferResponse(
    Guid Id,
    Guid FromAccountId,
    Guid ToAccountId,
    DateOnly Date,
    decimal Amount,
    string Currency,
    decimal ReceivedAmount,
    string ReceivedCurrency,
    string? Description)
{
    public static TransferResponse From(Transfer transfer)
    {
        ArgumentNullException.ThrowIfNull(transfer);

        return new TransferResponse(
            transfer.Id.Value,
            transfer.FromAccountId.Value,
            transfer.ToAccountId.Value,
            transfer.Date,
            transfer.Amount.Amount,
            transfer.Amount.Currency.Code,
            transfer.ReceivedAmount.Amount,
            transfer.ReceivedAmount.Currency.Code,
            transfer.Description);
    }
}