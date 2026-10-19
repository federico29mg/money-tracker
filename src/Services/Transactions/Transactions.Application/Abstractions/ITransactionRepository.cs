using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Abstractions;

public interface ITransactionRepository
{
    // Returns null when the transaction doesn't exist or belongs to another user, so callers can't tell the two apart.
    Task<Transaction?> GetByIdAsync(TransactionId id, UserId userId, CancellationToken cancellationToken);

    // Newest first: by date, then by id for transactions on the same day.
    Task<IReadOnlyList<Transaction>> ListAsync(
        UserId userId,
        TransactionFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(UserId userId, TransactionFilter filter, CancellationToken cancellationToken);

    void Add(Transaction transaction);
}