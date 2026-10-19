using Transactions.Application.Common;
using Transactions.Application.Features.Categories;
using Transactions.Application.Features.Categories.GetCategory;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Categories;

public class GetCategoryHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Category _category = Category.Create(Owner, "Salary", TransactionType.Income);
    private readonly GetCategoryHandler _handler;
    private readonly InMemoryCategoryRepository _repository = new();

    public GetCategoryHandlerTests()
    {
        _repository.Add(_category);
        _handler = new GetCategoryHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_OwnCategory_ReturnsMappedResponse()
    {
        var response = await _handler.HandleAsync(
            new GetCategoryQuery(Owner.Value, _category.Id.Value), TestContext.Current.CancellationToken);

        Assert.Equal(new CategoryResponse(_category.Id.Value, "Salary", TransactionType.Income, false), response);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersCategory_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new GetCategoryQuery(Guid.NewGuid(), _category.Id.Value), TestContext.Current.CancellationToken));
}