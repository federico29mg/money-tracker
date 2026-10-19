using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Abstractions;

public interface IAccountRepository
{
    // Returns null when the account doesn't exist or belongs to another user, so callers can't tell the two apart.
    Task<Account?> GetByIdAsync(AccountId id, UserId userId, CancellationToken cancellationToken);

    // Ordered by name.
    Task<IReadOnlyList<Account>> ListAsync(UserId userId, bool includeArchived, CancellationToken cancellationToken);

    void Add(Account account);
}