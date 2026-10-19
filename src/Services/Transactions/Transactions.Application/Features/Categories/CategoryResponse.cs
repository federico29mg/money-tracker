using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Categories;

public sealed record CategoryResponse(Guid Id, string Name, TransactionType Type, bool IsArchived)
{
    public static CategoryResponse From(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return new CategoryResponse(category.Id.Value, category.Name, category.Type, category.IsArchived);
    }
}