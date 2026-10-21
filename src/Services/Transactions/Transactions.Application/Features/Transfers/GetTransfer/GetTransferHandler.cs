using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.Application.Features.Transfers.GetTransfer;

public sealed class GetTransferHandler(ITransferRepository transferRepository)
{
    public async Task<TransferResponse> HandleAsync(GetTransferQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var transfer = await transferRepository.GetByIdAsync(
                           TransferId.From(query.TransferId), UserId.From(query.UserId), cancellationToken)
                       ?? throw new NotFoundException($"Transfer '{query.TransferId}' was not found.");

        return TransferResponse.From(transfer);
    }
}