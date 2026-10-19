using Transactions.Application.Abstractions;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.UnitTests.Fakes;

public sealed class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly Dictionary<TransactionId, Transaction> _transactions = [];

    public IReadOnlyCollection<Transaction> StoredTransactions => _transactions.Values;

    public Task<Transaction?> GetByIdAsync(TransactionId id, UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult(_transactions.TryGetValue(id, out var transaction) && transaction.UserId == userId
            ? transaction
            : null);

    public Task<IReadOnlyList<Transaction>> ListAsync(
        UserId userId,
        TransactionFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Transaction> transactions =
        [
            .. Matching(userId, filter)
                .OrderByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Id.Value)
                .Skip(skip)
                .Take(take)
        ];

        return Task.FromResult(transactions);
    }

    public Task<int> CountAsync(UserId userId, TransactionFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult(Matching(userId, filter).Count());

    public void Add(Transaction transaction) => _transactions.Add(transaction.Id, transaction);

    private IEnumerable<Transaction> Matching(UserId userId, TransactionFilter filter) =>
        _transactions.Values.Where(transaction =>
            transaction.UserId == userId
            && (filter.AccountId is null || transaction.AccountId == filter.AccountId)
            && (filter.CategoryId is null || transaction.CategoryId == filter.CategoryId)
            && (filter.From is null || transaction.Date >= filter.From)
            && (filter.To is null || transaction.Date <= filter.To));
}