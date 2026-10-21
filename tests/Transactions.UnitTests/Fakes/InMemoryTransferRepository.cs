using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.UnitTests.Fakes;

public sealed class InMemoryTransferRepository : ITransferRepository
{
    private readonly Dictionary<TransferId, Transfer> _transfers = [];

    public IReadOnlyCollection<Transfer> StoredTransfers => _transfers.Values;

    public Task<Transfer?> GetByIdAsync(TransferId id, UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult(_transfers.TryGetValue(id, out var transfer) && transfer.UserId == userId ? transfer : null);

    public Task<IReadOnlyList<Transfer>> ListAsync(
        UserId userId,
        TransferFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Transfer> transfers =
        [
            .. Matching(userId, filter)
                .OrderByDescending(transfer => transfer.Date)
                .ThenByDescending(transfer => transfer.Id.Value)
                .Skip(skip)
                .Take(take)
        ];

        return Task.FromResult(transfers);
    }

    public Task<int> CountAsync(UserId userId, TransferFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult(Matching(userId, filter).Count());

    public Task<IReadOnlyDictionary<AccountId, decimal>> GetNetByAccountAsync(
        IReadOnlyCollection<AccountId> accountIds,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var net = new Dictionary<AccountId, decimal>();

        foreach (var transfer in _transfers.Values.Where(transfer => transfer.UserId == userId))
        {
            if (accountIds.Contains(transfer.FromAccountId))
            {
                net[transfer.FromAccountId] = net.GetValueOrDefault(transfer.FromAccountId) - transfer.Amount.Amount;
            }

            if (accountIds.Contains(transfer.ToAccountId))
            {
                net[transfer.ToAccountId] =
                    net.GetValueOrDefault(transfer.ToAccountId) + transfer.ReceivedAmount.Amount;
            }
        }

        return Task.FromResult<IReadOnlyDictionary<AccountId, decimal>>(net);
    }

    public void Add(Transfer transfer) => _transfers.Add(transfer.Id, transfer);

    private IEnumerable<Transfer> Matching(UserId userId, TransferFilter filter) =>
        _transfers.Values.Where(transfer =>
            transfer.UserId == userId
            && (filter.AccountId is null
                || transfer.FromAccountId == filter.AccountId
                || transfer.ToAccountId == filter.AccountId)
            && (filter.From is null || transfer.Date >= filter.From)
            && (filter.To is null || transfer.Date <= filter.To));
}