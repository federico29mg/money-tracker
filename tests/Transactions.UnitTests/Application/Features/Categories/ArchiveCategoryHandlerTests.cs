using Transactions.Application.Common;
using Transactions.Application.Features.Categories.ArchiveCategory;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Categories;

public class ArchiveCategoryHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Category _category = Category.Create(Owner, "Gym", TransactionType.Expense);
    private readonly ArchiveCategoryHandler _handler;
    private readonly InMemoryCategoryRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public ArchiveCategoryHandlerTests()
    {
        _repository.Add(_category);
        _handler = new ArchiveCategoryHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task HandleAsync_OwnCategory_ArchivesAndSaves()
    {
        await _handler.HandleAsync(
            new ArchiveCategoryCommand(Owner.Value, _category.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(_category.IsArchived);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersCategory_ThrowsNotFoundAndChangesNothing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new ArchiveCategoryCommand(Guid.NewGuid(), _category.Id.Value),
                TestContext.Current.CancellationToken));

        Assert.False(_category.IsArchived);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}