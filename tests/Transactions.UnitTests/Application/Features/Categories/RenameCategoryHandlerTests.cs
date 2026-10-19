using Transactions.Application.Common;
using Transactions.Application.Features.Categories.RenameCategory;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Categories;

public class RenameCategoryHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Category _gym = Category.Create(Owner, "Gym", TransactionType.Expense);
    private readonly RenameCategoryHandler _handler;
    private readonly InMemoryCategoryRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public RenameCategoryHandlerTests()
    {
        _repository.Add(_gym);
        _repository.Add(Category.Create(Owner, "Health", TransactionType.Expense));
        _handler = new RenameCategoryHandler(_repository, _unitOfWork);
    }

    private Task RenameAsync(string name, Guid? userId = null) =>
        _handler.HandleAsync(
            new RenameCategoryCommand(userId ?? Owner.Value, _gym.Id.Value, name),
            TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_NewName_RenamesAndSaves()
    {
        await RenameAsync("Sports");

        Assert.Equal("Sports", _gym.Name);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_OwnNameWithDifferentCase_Renames()
    {
        await RenameAsync("GYM");

        Assert.Equal("GYM", _gym.Name);
    }

    [Fact]
    public async Task HandleAsync_NameOfOtherCategory_ThrowsConflictAndKeepsName()
    {
        await Assert.ThrowsAsync<ConflictException>(() => RenameAsync(" health "));

        Assert.Equal("Gym", _gym.Name);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_BlankName_ThrowsDomainException() =>
        await Assert.ThrowsAsync<DomainException>(() => RenameAsync("  "));

    [Fact]
    public async Task HandleAsync_OtherUsersCategory_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() => RenameAsync("Sports", Guid.NewGuid()));
}