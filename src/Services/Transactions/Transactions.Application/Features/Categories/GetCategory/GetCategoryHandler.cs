using Transactions.Application.Abstractions;

namespace Transactions.Application.Features.Categories.GetCategory;

public sealed class GetCategoryHandler(ICategoryRepository categoryRepository)
{
    public async Task<CategoryResponse> HandleAsync(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var category = await categoryRepository.GetRequiredAsync(query.CategoryId, query.UserId, cancellationToken);

        return CategoryResponse.From(category);
    }
}