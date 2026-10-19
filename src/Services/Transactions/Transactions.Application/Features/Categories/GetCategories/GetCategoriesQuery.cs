using Transactions.Domain.Common;

namespace Transactions.Application.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery(Guid UserId, TransactionType? Type = null, bool IncludeArchived = false);