namespace Transactions.Application.Features.Categories.GetCategory;

public sealed record GetCategoryQuery(Guid UserId, Guid CategoryId);