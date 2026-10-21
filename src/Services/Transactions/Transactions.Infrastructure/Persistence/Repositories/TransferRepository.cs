using Microsoft.EntityFrameworkCore;

using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.Infrastructure.Persistence.Repositories;

internal sealed class TransferRepository(TransactionsDbContext dbContext) : ITransferRepository
{
    private IQueryable<Transfer> Transfers => dbContext.Set<Transfer>();

    public Task<Transfer?> GetByIdAsync(TransferId id, UserId userId, CancellationToken cancellationToken) =>
        Transfers.FirstOrDefaultAsync(transfer => transfer.Id == id && transfer.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Transfer>> ListAsync(
        UserId userId,
        TransferFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await Matching(userId, filter)
            .OrderByDescending(transfer => transfer.Date)
            .ThenByDescending(transfer => transfer.Id)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(UserId userId, TransferFilter filter, CancellationToken cancellationToken) =>
        Matching(userId, filter).CountAsync(cancellationToken);

    // Two grouped sums, money in and money out, combined per account.
    public async Task<IReadOnlyDictionary<AccountId, decimal>> GetNetByAccountAsync(
        IReadOnlyCollection<AccountId> accountIds,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var ofUser = Transfers.Where(transfer => transfer.UserId == userId);

        var incoming = await ofUser
            .Where(transfer => accountIds.Contains(transfer.ToAccountId))
            .GroupBy(transfer => transfer.ToAccountId)
            .Select(account => new { AccountId = account.Key, Total = account.Sum(t => t.ReceivedAmount.Amount) })
            .ToListAsync(cancellationToken);

        var outgoing = await ofUser
            .Where(transfer => accountIds.Contains(transfer.FromAccountId))
            .GroupBy(transfer => transfer.FromAccountId)
            .Select(account => new { AccountId = account.Key, Total = account.Sum(t => t.Amount.Amount) })
            .ToListAsync(cancellationToken);

        var net = incoming.ToDictionary(total => total.AccountId, total => total.Total);

        foreach (var total in outgoing)
        {
            net[total.AccountId] = net.GetValueOrDefault(total.AccountId) - total.Total;
        }

        return net;
    }

    public void Add(Transfer transfer) => dbContext.Set<Transfer>().Add(transfer);

    private IQueryable<Transfer> Matching(UserId userId, TransferFilter filter)
    {
        var query = Transfers.Where(transfer => transfer.UserId == userId);

        if (filter.AccountId is { } accountId)
        {
            query = query.Where(transfer => transfer.FromAccountId == accountId || transfer.ToAccountId == accountId);
        }

        if (filter.From is { } from)
        {
            query = query.Where(transfer => transfer.Date >= from);
        }

        if (filter.To is { } to)
        {
            query = query.Where(transfer => transfer.Date <= to);
        }

        return query;
    }
}