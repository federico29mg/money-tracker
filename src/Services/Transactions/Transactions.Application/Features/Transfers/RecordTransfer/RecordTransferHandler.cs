using Transactions.Application.Abstractions;
using Transactions.Application.Features.Accounts;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.Application.Features.Transfers.RecordTransfer;

public sealed class RecordTransferHandler(
    IAccountRepository accountRepository,
    ITransferRepository transferRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(RecordTransferCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var from = await accountRepository.GetRequiredAsync(command.FromAccountId, command.UserId, cancellationToken);
        var to = await accountRepository.GetRequiredAsync(command.ToAccountId, command.UserId, cancellationToken);

        // The currencies come from the accounts, so the client only sends numbers.
        var transfer = Transfer.Record(
            from,
            to,
            Money.Of(command.Amount, from.Currency),
            command.ReceivedAmount is { } received ? Money.Of(received, to.Currency) : null,
            command.Date,
            command.Description);

        transferRepository.Add(transfer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transfer.Id.Value;
    }
}