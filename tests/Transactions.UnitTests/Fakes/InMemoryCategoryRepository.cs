using Transactions.Application.Abstractions;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.UnitTests.Fakes;

public sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly Dictionary<CategoryId, Category> _categories = [];

    public IReadOnlyCollection<Category> StoredCategories => _categories.Values;

    public Task<Category?> GetByIdAsync(CategoryId id, UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult(_categories.TryGetValue(id, out var category) && category.UserId == userId ? category : null);

    public Task<IReadOnlyList<Category>> ListAsync(
        UserId userId,
        TransactionType? type,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories =
        [
            .. _categories.Values
                .Where(category => category.UserId == userId
                                   && (type is null || category.Type == type)
                                   && (includeArchived || !category.IsArchived))
                .OrderBy(category => category.Name, StringComparer.Ordinal)
        ];

        return Task.FromResult(categories);
    }

    public Task<bool> NameExistsAsync(
        UserId userId,
        TransactionType type,
        string name,
        CategoryId? excludingId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_categories.Values.Any(category =>
            category.UserId == userId
            && category.Type == type
            && category.Id != excludingId
            && string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(Category category) => _categories.Add(category.Id, category);
}