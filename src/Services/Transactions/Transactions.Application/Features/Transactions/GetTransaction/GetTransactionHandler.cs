using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Features.Transactions.GetTransaction;

public sealed class GetTransactionHandler(ITransactionRepository transactionRepository)
{
    public async Task<TransactionResponse> HandleAsync(GetTransactionQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var transaction = await transactionRepository.GetByIdAsync(
                              TransactionId.From(query.TransactionId), UserId.From(query.UserId), cancellationToken)
                          ?? throw new NotFoundException($"Transaction '{query.TransactionId}' was not found.");

        return TransactionResponse.From(transaction);
    }
}