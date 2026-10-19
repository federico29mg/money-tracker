namespace Transactions.Application.Features.Categories.RenameCategory;

public sealed record RenameCategoryCommand(Guid UserId, Guid CategoryId, string Name);