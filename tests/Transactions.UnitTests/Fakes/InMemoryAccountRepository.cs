using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.UnitTests.Fakes;

public sealed class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<AccountId, Account> _accounts = [];

    public IReadOnlyCollection<Account> StoredAccounts => _accounts.Values;

    public Task<Account?> GetByIdAsync(AccountId id, UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult(_accounts.TryGetValue(id, out var account) && account.UserId == userId ? account : null);

    public Task<IReadOnlyList<Account>> ListAsync(
        UserId userId,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Account> accounts =
        [
            .. _accounts.Values
                .Where(account => account.UserId == userId && (includeArchived || !account.IsArchived))
                .OrderBy(account => account.Name, StringComparer.Ordinal)
        ];

        return Task.FromResult(accounts);
    }

    public void Add(Account account) => _accounts.Add(account.Id, account);
}