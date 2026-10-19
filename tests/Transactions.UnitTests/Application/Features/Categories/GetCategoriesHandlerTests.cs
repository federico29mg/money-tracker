using Transactions.Application.Features.Categories.GetCategories;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Categories;

public class GetCategoriesHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly GetCategoriesHandler _handler;
    private readonly InMemoryCategoryRepository _repository = new();

    public GetCategoriesHandlerTests()
    {
        var archived = Category.Create(Owner, "Gym", TransactionType.Expense);
        archived.Archive();

        _repository.Add(Category.Create(Owner, "Salary", TransactionType.Income));
        _repository.Add(Category.Create(Owner, "Rent", TransactionType.Expense));
        _repository.Add(Category.Create(Owner, "Groceries", TransactionType.Expense));
        _repository.Add(archived);
        _repository.Add(Category.Create(UserId.From(Guid.NewGuid()), "Someone else's", TransactionType.Expense));

        _handler = new GetCategoriesHandler(_repository);
    }

    private async Task<IEnumerable<string>> NamesAsync(GetCategoriesQuery query) =>
        (await _handler.HandleAsync(query, TestContext.Current.CancellationToken)).Select(category => category.Name);

    [Fact]
    public async Task HandleAsync_Default_ReturnsOwnActiveCategoriesOfBothTypesByName() =>
        Assert.Equal(["Groceries", "Rent", "Salary"], await NamesAsync(new GetCategoriesQuery(Owner.Value)));

    [Fact]
    public async Task HandleAsync_TypeFilter_ReturnsOnlyThatType() =>
        Assert.Equal(
            ["Groceries", "Rent"],
            await NamesAsync(new GetCategoriesQuery(Owner.Value, TransactionType.Expense)));

    [Fact]
    public async Task HandleAsync_IncludeArchived_AlsoReturnsArchivedCategories() =>
        Assert.Equal(
            ["Groceries", "Gym", "Rent"],
            await NamesAsync(new GetCategoriesQuery(Owner.Value, TransactionType.Expense, IncludeArchived: true)));
}