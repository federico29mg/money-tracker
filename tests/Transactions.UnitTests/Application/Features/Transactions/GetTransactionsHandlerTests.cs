using Transactions.Application.Common;
using Transactions.Application.Features.Transactions;
using Transactions.Application.Features.Transactions.GetTransactions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transactions;

public class GetTransactionsHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _card = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly Account _cash = Account.Open(Owner, "Cash", AccountType.Cash, Money.Zero(Currency.Cop));
    private readonly Category _groceries = Category.Create(Owner, "Groceries", TransactionType.Expense);
    private readonly GetTransactionsHandler _handler;
    private readonly InMemoryTransactionRepository _repository = new();
    private readonly Category _transport = Category.Create(Owner, "Transport", TransactionType.Expense);

    public GetTransactionsHandlerTests()
    {
        Add(_card, _groceries, 1, "Supermarket");
        Add(_cash, _transport, 5, "Bus");
        Add(_card, _transport, 10, "Taxi");
        Add(_card, _groceries, 15, "Bakery");

        var someoneElse = UserId.From(Guid.NewGuid());
        Add(
            Account.Open(someoneElse, "Theirs", AccountType.Cash, Money.Zero(Currency.Cop)),
            Category.Create(someoneElse, "Theirs", TransactionType.Expense),
            20,
            "Not mine");

        _handler = new GetTransactionsHandler(_repository);
    }

    private void Add(Account account, Category category, int day, string name) =>
        _repository.Add(Transaction.Record(
            account, category, TransactionType.Expense, Money.Of(1_000m, Currency.Cop), new DateOnly(2026, 10, day),
            name, null));

    private Task<PagedResponse<TransactionResponse>> GetAsync(GetTransactionsQuery query) =>
        _handler.HandleAsync(query, TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_NoFilters_ReturnsOwnTransactionsNewestFirst()
    {
        var page = await GetAsync(new GetTransactionsQuery(Owner.Value));

        Assert.Equal(["Bakery", "Taxi", "Bus", "Supermarket"], page.Items.Select(transaction => transaction.Name));
        Assert.Equal(4, page.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_AccountFilter_ReturnsOnlyThatAccount()
    {
        var page = await GetAsync(new GetTransactionsQuery(Owner.Value, AccountId: _cash.Id.Value));

        Assert.Equal(["Bus"], page.Items.Select(transaction => transaction.Name));
    }

    [Fact]
    public async Task HandleAsync_CategoryFilter_ReturnsOnlyThatCategory()
    {
        var page = await GetAsync(new GetTransactionsQuery(Owner.Value, CategoryId: _transport.Id.Value));

        Assert.Equal(["Taxi", "Bus"], page.Items.Select(transaction => transaction.Name));
    }

    [Fact]
    public async Task HandleAsync_DateRange_IncludesBothEnds()
    {
        var page = await GetAsync(new GetTransactionsQuery(
            Owner.Value, From: new DateOnly(2026, 10, 5), To: new DateOnly(2026, 10, 10)));

        Assert.Equal(["Taxi", "Bus"], page.Items.Select(transaction => transaction.Name));
    }

    [Fact]
    public async Task HandleAsync_SecondPage_ReturnsNextItemsAndTotals()
    {
        var page = await GetAsync(new GetTransactionsQuery(Owner.Value, Page: 2, PageSize: 3));

        Assert.Equal(["Supermarket"], page.Items.Select(transaction => transaction.Name));
        Assert.Equal(2, page.Page);
        Assert.Equal(3, page.PageSize);
        Assert.Equal(4, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, GetTransactionsQuery.MaxPageSize + 1)]
    public async Task HandleAsync_InvalidPaging_ThrowsValidationException(int page, int pageSize) =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            GetAsync(new GetTransactionsQuery(Owner.Value, Page: page, PageSize: pageSize)));

    [Fact]
    public async Task HandleAsync_FromAfterTo_ThrowsValidationException()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            GetAsync(new GetTransactionsQuery(
                Owner.Value, From: new DateOnly(2026, 10, 31), To: new DateOnly(2026, 10, 1))));

        Assert.Contains(nameof(GetTransactionsQuery.From), exception.Errors.Keys);
    }
}