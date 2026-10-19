using Transactions.Application.Abstractions;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Categories.GetCategories;

public sealed class GetCategoriesHandler(ICategoryRepository categoryRepository)
{
    public async Task<IReadOnlyList<CategoryResponse>> HandleAsync(
        GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await categoryRepository.ListAsync(
            UserId.From(query.UserId), query.Type, query.IncludeArchived, cancellationToken);

        return [.. categories.Select(CategoryResponse.From)];
    }
}