using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Abstractions;

public interface ITransactionRepository
{
    // Returns null when the transaction doesn't exist or belongs to another user, so callers can't tell the two apart.
    Task<Transaction?> GetByIdAsync(TransactionId id, UserId userId, CancellationToken cancellationToken);
    
    // Only the user's own transactions; ids that don't match are left out.
    Task<IReadOnlyList<Transaction>> ListByIdsAsync(
        IReadOnlyCollection<TransactionId> ids,
        UserId userId,
        CancellationToken cancellationToken);

    // Newest first: by date, then by id for transactions on the same day.
    Task<IReadOnlyList<Transaction>> ListAsync(
        UserId userId,
        TransactionFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(UserId userId, TransactionFilter filter, CancellationToken cancellationToken);
    
    // Unsettled transactions of one account, oldest first.
    Task<IReadOnlyList<Transaction>> ListPendingAsync(
        AccountId accountId,
        UserId userId,
        CancellationToken cancellationToken);
    
    // Per account: settled income minus settled expenses, in the account's currency.
    // Pending: per account and currency, unsettled income minus unsettled expenses.
    // Accounts without transactions are left out.
    Task<IReadOnlyList<AccountTotals>> GetTotalsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        UserId userId,
        CancellationToken cancellationToken);

    void Add(Transaction transaction);
}