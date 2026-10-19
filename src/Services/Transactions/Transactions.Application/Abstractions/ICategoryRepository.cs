using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Abstractions;

public interface ICategoryRepository
{
    // Returns null when the category doesn't exist or belongs to another user, so callers can't tell the two apart.
    Task<Category?> GetByIdAsync(CategoryId id, UserId userId, CancellationToken cancellationToken);

    // Ordered by name. A null type returns both income and expense categories.
    Task<IReadOnlyList<Category>> ListAsync(
        UserId userId,
        TransactionType? type,
        bool includeArchived,
        CancellationToken cancellationToken);

    // Case-insensitive, archived categories included. excludingId lets a category keep its own name when renamed.
    Task<bool> NameExistsAsync(
        UserId userId,
        TransactionType type,
        string name,
        CategoryId? excludingId,
        CancellationToken cancellationToken);

    void Add(Category category);
}