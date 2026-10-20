using Microsoft.EntityFrameworkCore;

using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Infrastructure.Persistence.Repositories;

internal sealed class TransactionRepository(TransactionsDbContext dbContext) : ITransactionRepository
{
    private IQueryable<Transaction> Transactions => dbContext.Set<Transaction>();

    public Task<Transaction?> GetByIdAsync(TransactionId id, UserId userId, CancellationToken cancellationToken) =>
        Transactions.FirstOrDefaultAsync(
            transaction => transaction.Id == id && transaction.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> ListByIdsAsync(
        IReadOnlyCollection<TransactionId> ids,
        UserId userId,
        CancellationToken cancellationToken) =>
        await Transactions
            .Where(transaction => transaction.UserId == userId && ids.Contains(transaction.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Transaction>> ListAsync(
        UserId userId,
        TransactionFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await Matching(userId, filter)
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Id)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(UserId userId, TransactionFilter filter, CancellationToken cancellationToken) =>
        Matching(userId, filter).CountAsync(cancellationToken);

    // ExchangeRate is set exactly when a transaction is settled, and it's a plain column, so it's the cheapest test.
    public async Task<IReadOnlyList<Transaction>> ListPendingAsync(
        AccountId accountId,
        UserId userId,
        CancellationToken cancellationToken) =>
        await Transactions
            .Where(transaction => transaction.UserId == userId
                                  && transaction.AccountId == accountId
                                  && transaction.ExchangeRate == null)
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Two grouped queries: the database adds the amounts up, so no transactions are loaded into memory.
    public async Task<IReadOnlyList<AccountTotals>> GetTotalsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var ofAccounts = Transactions
            .Where(transaction => transaction.UserId == userId && accountIds.Contains(transaction.AccountId));

        var settled = await ofAccounts
            .Where(transaction => transaction.ExchangeRate != null)
            .GroupBy(transaction => transaction.AccountId)
            .Select(account => new
            {
                AccountId = account.Key,
                Net = account.Sum(transaction => transaction.Type == TransactionType.Income
                    ? transaction.SettledAmount!.Amount
                    : -transaction.SettledAmount!.Amount)
            })
            .ToDictionaryAsync(total => total.AccountId, total => total.Net, cancellationToken);

        var pending = await ofAccounts
            .Where(transaction => transaction.ExchangeRate == null)
            .GroupBy(transaction => new { transaction.AccountId, transaction.Amount.Currency })
            .Select(group => new
            {
                group.Key.AccountId,
                group.Key.Currency,
                Net = group.Sum(transaction => transaction.Type == TransactionType.Income
                    ? transaction.Amount.Amount
                    : -transaction.Amount.Amount)
            })
            .ToListAsync(cancellationToken);

        return 
            [.. 
                settled.Keys
                    .Union(pending.Select(total => total.AccountId))
                    .Select(accountId => new AccountTotals(
                        accountId,
                        settled.GetValueOrDefault(accountId),
                        [.. pending
                            .Where(total => total.AccountId == accountId)
                            .OrderBy(total => total.Currency.Code, StringComparer.Ordinal)
                            .Select(total => Money.Of(total.Net, total.Currency))
                        ]))
            ];
    }

    public void Add(Transaction transaction) => dbContext.Set<Transaction>().Add(transaction);

    private IQueryable<Transaction> Matching(UserId userId, TransactionFilter filter)
    {
        var query = Transactions.Where(transaction => transaction.UserId == userId);

        if (filter.AccountId is { } accountId)
        {
            query = query.Where(transaction => transaction.AccountId == accountId);
        }

        if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(transaction => transaction.CategoryId == categoryId);
        }

        if (filter.From is { } from)
        {
            query = query.Where(transaction => transaction.Date >= from);
        }

        if (filter.To is { } to)
        {
            query = query.Where(transaction => transaction.Date <= to);
        }

        return query;
    }
}