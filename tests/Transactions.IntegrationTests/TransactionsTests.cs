using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

using Transactions.Api.Contracts;
using Transactions.Application.Common;
using Transactions.Application.Features.Transactions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.IntegrationTests.Support;

namespace Transactions.IntegrationTests;

public class TransactionsTests(TransactionsApiFixture fixture)
{
    [Fact]
    public async Task List_IsNewestFirstAndPaged()
    {
        using var client = fixture.CreateClient(Guid.NewGuid());
        var (account, category) = await SetUpAsync(client);

        foreach (var day in new[] { 1, 3, 2 })
        {
            await client.CreateAsync("/api/transactions", new RecordTransactionRequest(
                account, category, TransactionType.Expense, 1_000m, null, new DateOnly(2026, 11, day), $"Day {day}",
                null));
        }

        var firstPage = await client.GetJsonAsync<PagedResponse<TransactionResponse>>("/api/transactions?pageSize=2");
        var secondPage = await client.GetJsonAsync<PagedResponse<TransactionResponse>>(
            "/api/transactions?pageSize=2&page=2");

        Assert.Equal(["Day 3", "Day 2"], firstPage.Items.Select(transaction => transaction.Name));
        Assert.Equal(["Day 1"], secondPage.Items.Select(transaction => transaction.Name));
        Assert.Equal(3, firstPage.TotalCount);
    }

    [Fact]
    public async Task Record_InvalidAmount_ReturnsProblemDetails()
    {
        using var client = fixture.CreateClient(Guid.NewGuid());
        var (account, category) = await SetUpAsync(client);

        using var response = await client.PostJsonAsync("/api/transactions", new RecordTransactionRequest(
            account, category, TransactionType.Expense, 0m, null, new DateOnly(2026, 11, 4), "Nothing", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            HttpClientExtensions.JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal("Amount must be greater than zero.", problem?.Detail);
    }

    private static async Task<(Guid Account, Guid Category)> SetUpAsync(HttpClient client)
    {
        var account = await client.CreateAsync(
            "/api/accounts", new OpenAccountRequest("Cash", AccountType.Cash, "COP", 0m));
        var category = await client.CreateAsync(
            "/api/categories", new CreateCategoryRequest("Food", TransactionType.Expense));

        return (account, category);
    }
}