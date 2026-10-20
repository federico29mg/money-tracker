using Transactions.Application.Common;
using Transactions.Application.Features.Transactions.GetPendingTransactions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transactions;

public class GetPendingTransactionsHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly InMemoryAccountRepository _accounts = new();
    private readonly Account _card = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly GetPendingTransactionsHandler _handler;
    private readonly InMemoryTransactionRepository _transactions = new();

    public GetPendingTransactionsHandlerTests()
    {
        var otherCard = Account.Open(Owner, "Mastercard", AccountType.CreditCard, Money.Zero(Currency.Cop));
        _accounts.Add(_card);
        _accounts.Add(otherCard);

        Record(_card, TransactionType.Expense, Money.Of(50m, Currency.Usd), 3, "Amazon");
        Record(_card, TransactionType.Expense, Money.Of(20m, Currency.Usd), 1, "Netflix");
        Record(_card, TransactionType.Income, Money.Of(5m, Currency.Usd), 2, "Refund");
        Record(_card, TransactionType.Expense, Money.Of(85_000m, Currency.Cop), 4, "Supermarket");
        Record(_card, TransactionType.Expense, Money.Of(10m, Currency.Usd), 5, "Already paid").Settle(_card, 4000m);
        Record(otherCard, TransactionType.Expense, Money.Of(99m, Currency.Usd), 6, "Other card");

        _handler = new GetPendingTransactionsHandler(_accounts, _transactions);
    }

    private Transaction Record(Account account, TransactionType type, Money amount, int day, string name)
    {
        var category = Category.Create(Owner, type.ToString(), type);
        var transaction = Transaction.Record(account, category, type, amount, new DateOnly(2026, 10, day), name, null);
        _transactions.Add(transaction);

        return transaction;
    }

    [Fact]
    public async Task HandleAsync_OwnAccount_GroupsUnsettledTransactionsByCurrency()
    {
        var response = await _handler.HandleAsync(
            new GetPendingTransactionsQuery(Owner.Value, _card.Id.Value), TestContext.Current.CancellationToken);

        Assert.Equal(_card.Id.Value, response.AccountId);
        Assert.Equal("COP", response.AccountCurrency);

        var usd = Assert.Single(response.Groups);
        Assert.Equal("USD", usd.Currency);
        Assert.Equal(-50m - 20m + 5m, usd.Total);
        Assert.Equal(["Netflix", "Refund", "Amazon"], usd.Transactions.Select(transaction => transaction.Name));
    }

    [Fact]
    public async Task HandleAsync_NothingPending_ReturnsNoGroups()
    {
        var cash = Account.Open(Owner, "Cash", AccountType.Cash, Money.Zero(Currency.Cop));
        _accounts.Add(cash);

        var response = await _handler.HandleAsync(
            new GetPendingTransactionsQuery(Owner.Value, cash.Id.Value), TestContext.Current.CancellationToken);

        Assert.Empty(response.Groups);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccount_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new GetPendingTransactionsQuery(Guid.NewGuid(), _card.Id.Value),
                TestContext.Current.CancellationToken));
}