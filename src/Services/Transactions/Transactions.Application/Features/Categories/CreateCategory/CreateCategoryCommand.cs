using Transactions.Domain.Common;

namespace Transactions.Application.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(Guid UserId, string Name, TransactionType Type);