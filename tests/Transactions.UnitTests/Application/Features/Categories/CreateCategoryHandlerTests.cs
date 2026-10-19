using Transactions.Application.Common;
using Transactions.Application.Features.Categories.CreateCategory;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Categories;

public class CreateCategoryHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly CreateCategoryHandler _handler;
    private readonly InMemoryCategoryRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public CreateCategoryHandlerTests() => _handler = new CreateCategoryHandler(_repository, _unitOfWork);

    private Task<Guid> CreateAsync(
        string name,
        TransactionType type = TransactionType.Expense,
        UserId? userId = null) =>
        _handler.HandleAsync(
            new CreateCategoryCommand((userId ?? Owner).Value, name, type), TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_NewName_StoresCategoryAndReturnsId()
    {
        var id = await CreateAsync("  Groceries  ");

        var category = Assert.Single(_repository.StoredCategories);
        Assert.Equal(id, category.Id.Value);
        Assert.Equal(Owner, category.UserId);
        Assert.Equal("Groceries", category.Name);
        Assert.Equal(TransactionType.Expense, category.Type);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("Groceries")]
    [InlineData("GROCERIES")]
    [InlineData("  groceries ")]
    public async Task HandleAsync_DuplicateNameForSameType_ThrowsConflictAndSavesNothing(string name)
    {
        await CreateAsync("Groceries");

        await Assert.ThrowsAsync<ConflictException>(() => CreateAsync(name));

        Assert.Single(_repository.StoredCategories);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_NameOfArchivedCategory_ThrowsConflictException()
    {
        var archived = Category.Create(Owner, "Gym", TransactionType.Expense);
        archived.Archive();
        _repository.Add(archived);

        await Assert.ThrowsAsync<ConflictException>(() => CreateAsync("Gym"));
    }

    [Fact]
    public async Task HandleAsync_SameNameForOtherType_CreatesCategory()
    {
        await CreateAsync("Freelance");

        await CreateAsync("Freelance", TransactionType.Income);

        Assert.Equal(2, _repository.StoredCategories.Count);
    }

    [Fact]
    public async Task HandleAsync_SameNameForOtherUser_CreatesCategory()
    {
        await CreateAsync("Groceries");

        await CreateAsync("Groceries", userId: UserId.From(Guid.NewGuid()));

        Assert.Equal(2, _repository.StoredCategories.Count);
    }

    [Fact]
    public async Task HandleAsync_BlankName_ThrowsDomainException() =>
        await Assert.ThrowsAsync<DomainException>(() => CreateAsync("   "));
}