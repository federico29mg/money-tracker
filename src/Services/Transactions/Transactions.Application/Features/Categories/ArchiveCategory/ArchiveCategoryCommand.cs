namespace Transactions.Application.Features.Categories.ArchiveCategory;

public sealed record ArchiveCategoryCommand(Guid UserId, Guid CategoryId);