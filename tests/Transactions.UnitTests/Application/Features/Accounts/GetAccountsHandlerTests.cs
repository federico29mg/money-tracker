using Transactions.Application.Features.Accounts.GetAccounts;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Accounts;

public class GetAccountsHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _cash = Account.Open(Owner, "Cash", AccountType.Cash, Money.Of(100_000m, Currency.Cop));
    private readonly GetAccountsHandler _handler;
    private readonly InMemoryAccountRepository _repository = new();
    private readonly InMemoryTransactionRepository _transactions = new();
    private readonly Account _visa = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));

    public GetAccountsHandlerTests()
    {
        var archived = Account.Open(Owner, "Old savings", AccountType.Savings, Money.Zero(Currency.Cop));
        archived.Archive();

        _repository.Add(_visa);
        _repository.Add(_cash);
        _repository.Add(archived);
        _repository.Add(Account.Open(
            UserId.From(Guid.NewGuid()), "Someone else's", AccountType.Cash, Money.Zero(Currency.Cop)));

        _handler = new GetAccountsHandler(_repository, _transactions);
    }

    [Fact]
    public async Task HandleAsync_Default_ReturnsOwnActiveAccountsByName()
    {
        var accounts = await _handler.HandleAsync(
            new GetAccountsQuery(Owner.Value), TestContext.Current.CancellationToken);

        Assert.Equal(["Cash", "Visa"], accounts.Select(account => account.Name));
    }

    [Fact]
    public async Task HandleAsync_IncludeArchived_AlsoReturnsArchivedAccounts()
    {
        var accounts = await _handler.HandleAsync(
            new GetAccountsQuery(Owner.Value, IncludeArchived: true), TestContext.Current.CancellationToken);

        Assert.Equal(["Cash", "Old savings", "Visa"], accounts.Select(account => account.Name));
    }

    [Fact]
    public async Task HandleAsync_UserWithoutAccounts_ReturnsEmptyList()
    {
        var accounts = await _handler.HandleAsync(
            new GetAccountsQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Empty(accounts);
    }

    [Fact]
    public async Task HandleAsync_AccountsWithTransactions_EachGetsItsOwnBalance()
    {
        var groceries = Category.Create(Owner, "Groceries", TransactionType.Expense);
        _transactions.Add(Transaction.Record(
            _visa, groceries, TransactionType.Expense, Money.Of(85_000m, Currency.Cop), new DateOnly(2026, 10, 19),
            "Supermarket", null));

        var accounts = await _handler.HandleAsync(
            new GetAccountsQuery(Owner.Value), TestContext.Current.CancellationToken);

        Assert.Equal([100_000m, -85_000m], accounts.Select(account => account.Balance));
    }
}