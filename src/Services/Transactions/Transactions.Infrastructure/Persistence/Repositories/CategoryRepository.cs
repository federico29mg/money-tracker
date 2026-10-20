using Microsoft.EntityFrameworkCore;

using Transactions.Application.Abstractions;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(TransactionsDbContext dbContext) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(CategoryId id, UserId userId, CancellationToken cancellationToken) =>
        dbContext.Set<Category>()
            .FirstOrDefaultAsync(category => category.Id == id && category.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAsync(
        UserId userId,
        TransactionType? type,
        bool includeArchived,
        CancellationToken cancellationToken) =>
        await dbContext.Set<Category>()
            .Where(category => category.UserId == userId
                               && (type == null || category.Type == type)
                               && (includeArchived || !category.IsArchived))
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

    // Case-insensitive through the database's default collation, matching the unique index.
    public Task<bool> NameExistsAsync(
        UserId userId,
        TransactionType type,
        string name,
        CategoryId? excludingId,
        CancellationToken cancellationToken) =>
        dbContext.Set<Category>()
            .AnyAsync(category => category.UserId == userId
                                  && category.Type == type
                                  && category.Name == name
                                  && (excludingId == null || category.Id != excludingId), cancellationToken);

    public void Add(Category category) => dbContext.Set<Category>().Add(category);
}