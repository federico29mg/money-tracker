using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.Application.Abstractions;

public interface ITransferRepository
{
    // Returns null when the transfer doesn't exist or belongs to another user, so callers can't tell the two apart.
    Task<Transfer?> GetByIdAsync(TransferId id, UserId userId, CancellationToken cancellationToken);

    // Newest first: by date, then by id for transfers on the same day.
    Task<IReadOnlyList<Transfer>> ListAsync(
        UserId userId,
        TransferFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(UserId userId, TransferFilter filter, CancellationToken cancellationToken);

    // Per account: what arrived minus what left, in the account's currency. Accounts without transfers are left out.
    Task<IReadOnlyDictionary<AccountId, decimal>> GetNetByAccountAsync(
        IReadOnlyCollection<AccountId> accountIds,
        UserId userId,
        CancellationToken cancellationToken);

    void Add(Transfer transfer);
}