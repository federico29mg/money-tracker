using Transactions.Application.Abstractions;

namespace Transactions.Application.Features.Categories.ArchiveCategory;

public sealed class ArchiveCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(ArchiveCategoryCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var category = await categoryRepository.GetRequiredAsync(command.CategoryId, command.UserId, cancellationToken);

        category.Archive();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}