using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Categories;

internal static class CategoryRepositoryExtensions
{
    public static async Task<Category> GetRequiredAsync(
        this ICategoryRepository repository,
        Guid categoryId,
        Guid userId,
        CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(CategoryId.From(categoryId), UserId.From(userId), cancellationToken)
        ?? throw new NotFoundException($"Category '{categoryId}' was not found.");

    public static async Task EnsureNameIsAvailableAsync(
        this ICategoryRepository repository,
        UserId userId,
        TransactionType type,
        string name,
        CategoryId? excludingId,
        CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(userId, type, name, excludingId, cancellationToken))
        {
            throw new ConflictException($"An {type} category named '{name}' already exists.");
        }
    }
}