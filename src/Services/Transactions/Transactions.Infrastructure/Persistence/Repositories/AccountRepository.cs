using Microsoft.EntityFrameworkCore;

using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Infrastructure.Persistence.Repositories;

internal sealed class AccountRepository(TransactionsDbContext dbContext) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(AccountId id, UserId userId, CancellationToken cancellationToken) =>
        dbContext.Set<Account>()
            .FirstOrDefaultAsync(account => account.Id == id && account.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(
        UserId userId,
        bool includeArchived,
        CancellationToken cancellationToken) =>
        await dbContext.Set<Account>()
            .Where(account => account.UserId == userId && (includeArchived || !account.IsArchived))
            .OrderBy(account => account.Name)
            .ToListAsync(cancellationToken);

    public void Add(Account account) => dbContext.Set<Account>().Add(account);
}