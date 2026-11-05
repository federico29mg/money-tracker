using System.Net;
using System.Net.Http.Json;

using Transactions.Api.Contracts;
using Transactions.Application.Features.Accounts;
using Transactions.Application.Features.Transfers.PayCreditCard;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.IntegrationTests.Support;

namespace Transactions.IntegrationTests;

public class AccountsTests(TransactionsApiFixture fixture)
{
    private static readonly DateOnly Date = new(2026, 11, 4);

    [Fact]
    public async Task Balance_CombinesOpeningBalanceTransactionsAndTransfers()
    {
        using var client = fixture.CreateClient(Guid.NewGuid());
        var checking = await client.CreateAsync(
            "/api/accounts", new OpenAccountRequest("Checking", AccountType.Checking, "COP", 1_000_000m));
        var savings = await client.CreateAsync(
            "/api/accounts", new OpenAccountRequest("Savings", AccountType.Savings, "COP", 0m));
        var salary = await client.CreateAsync(
            "/api/categories", new CreateCategoryRequest("Salary", TransactionType.Income));
        var food = await client.CreateAsync(
            "/api/categories", new CreateCategoryRequest("Food", TransactionType.Expense));

        await client.CreateAsync("/api/transactions", new RecordTransactionRequest(
            checking, salary, TransactionType.Income, 500_000m, null, Date, "Salary", null));
        await client.CreateAsync("/api/transactions", new RecordTransactionRequest(
            checking, food, TransactionType.Expense, 120_000m, null, Date, "Groceries", null));
        await client.CreateAsync("/api/transfers", new RecordTransferRequest(
            checking, savings, 200_000m, null, Date, "Savings"));

        var accounts = await client.GetJsonAsync<List<AccountResponse>>("/api/accounts");

        Assert.Equal(1_180_000m, accounts.Single(account => account.Id == checking).Balance);
        Assert.Equal(200_000m, accounts.Single(account => account.Id == savings).Balance);
    }

    [Fact]
    public async Task CardPayment_WithSettlement_ClearsDebtAndPendingCharges()
    {
        using var client = fixture.CreateClient(Guid.NewGuid());
        var checking = await client.CreateAsync(
            "/api/accounts", new OpenAccountRequest("Checking", AccountType.Checking, "COP", 1_000_000m));
        var card = await client.CreateAsync(
            "/api/accounts", new OpenAccountRequest("Visa", AccountType.CreditCard, "COP", 0m));
        var shopping = await client.CreateAsync(
            "/api/categories", new CreateCategoryRequest("Shopping", TransactionType.Expense));

        var amazon = await client.CreateAsync("/api/transactions", new RecordTransactionRequest(
            card, shopping, TransactionType.Expense, 50m, "USD", Date, "Amazon", null));
        await client.CreateAsync("/api/transactions", new RecordTransactionRequest(
            card, shopping, TransactionType.Expense, 85_000m, null, Date, "Supermarket", null));

        var beforePaying = await client.GetJsonAsync<AccountResponse>($"/api/accounts/{card}");
        Assert.Equal(-85_000m, beforePaying.Balance);
        Assert.Equal([new PendingAmountResponse("USD", -50m)], beforePaying.Pending);

        using var response = await client.PostJsonAsync($"/api/accounts/{card}/payments", new PayCreditCardRequest(
            checking, 290_000m, null, Date, null, new SettleTransactionsRequest([amazon], 4_100m, null)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PayCreditCardResponse>(
            HttpClientExtensions.JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal($"/api/transfers/{payment!.TransferId}", response.Headers.Location?.AbsolutePath);

        var afterPaying = await client.GetJsonAsync<AccountResponse>($"/api/accounts/{card}");
        Assert.Equal(0m, afterPaying.Balance);
        Assert.Empty(afterPaying.Pending);
        Assert.Equal(710_000m, (await client.GetJsonAsync<AccountResponse>($"/api/accounts/{checking}")).Balance);
    }
}