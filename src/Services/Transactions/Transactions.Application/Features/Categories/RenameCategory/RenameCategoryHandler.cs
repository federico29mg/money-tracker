using Transactions.Application.Abstractions;

namespace Transactions.Application.Features.Categories.RenameCategory;

public sealed class RenameCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(RenameCategoryCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var category = await categoryRepository.GetRequiredAsync(command.CategoryId, command.UserId, cancellationToken);

        // Checked before Rename so a conflict leaves the category untouched; a blank name is rejected by Rename.
        var name = command.Name?.Trim() ?? string.Empty;

        await categoryRepository.EnsureNameIsAvailableAsync(
            category.UserId, category.Type, name, category.Id, cancellationToken);

        category.Rename(name);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}