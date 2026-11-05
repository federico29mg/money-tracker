using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Transactions.Api.Contracts;
using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.IntegrationTests.Support;

namespace Transactions.IntegrationTests;

public class CategoriesTests(TransactionsApiFixture fixture)
{
    [Fact]
    public async Task Create_SameNameInAnotherCase_Returns409()
    {
        using var client = fixture.CreateClient(Guid.NewGuid());
        await client.CreateAsync("/api/categories", new CreateCategoryRequest("Groceries", TransactionType.Expense));

        using var response = await client.PostJsonAsync(
            "/api/categories", new CreateCategoryRequest("groceries", TransactionType.Expense));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_SameNameForTheOtherType_IsAllowed()
    {
        using var client = fixture.CreateClient(Guid.NewGuid());
        await client.CreateAsync("/api/categories", new CreateCategoryRequest("Gifts", TransactionType.Expense));

        await client.CreateAsync("/api/categories", new CreateCategoryRequest("Gifts", TransactionType.Income));
    }

    // Two defaults can only appear if two provisioning messages race past the handler's check;
    // the filtered unique index is what stops the second one.
    [Fact]
    public async Task SecondDefaultForTheSameType_IsRejectedByTheDatabase()
    {
        var userId = UserId.From(Guid.NewGuid());
        await SaveAsync(Category.CreateDefault(userId, TransactionType.Expense));

        var other = Category.Create(userId, "Other", TransactionType.Expense);
        other.MakeDefault();

        await Assert.ThrowsAsync<ConflictException>(() => SaveAsync(other));
    }

    private async Task SaveAsync(Category category)
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();

        scope.ServiceProvider.GetRequiredService<ICategoryRepository>().Add(category);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
            .SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}