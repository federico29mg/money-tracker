using Transactions.Application.Abstractions;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Created first so the domain validates and trims the name before it's compared.
        var category = Category.Create(UserId.From(command.UserId), command.Name, command.Type);

        await categoryRepository.EnsureNameIsAvailableAsync(
            category.UserId, category.Type, category.Name, excludingId: null, cancellationToken);

        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id.Value;
    }
}