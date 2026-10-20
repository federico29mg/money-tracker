using Transactions.Application.Common;
using Transactions.Application.Features.Accounts;
using Transactions.Application.Features.Accounts.GetAccount;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Accounts;

public class GetAccountHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());
    private static readonly DateOnly Date = new(2026, 10, 19);

    private readonly InMemoryAccountRepository _accounts = new();

    private readonly Account _card =
        Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Of(-250_000m, Currency.Cop));

    private readonly GetAccountHandler _handler;
    private readonly InMemoryTransactionRepository _transactions = new();

    public GetAccountHandlerTests()
    {
        _accounts.Add(_card);
        _handler = new GetAccountHandler(_accounts, _transactions);
    }

    private Task<AccountResponse> GetAsync() =>
        _handler.HandleAsync(new GetAccountQuery(Owner.Value, _card.Id.Value), TestContext.Current.CancellationToken);

    private Transaction Record(TransactionType type, Money amount)
    {
        var category = Category.Create(Owner, type.ToString(), type);
        var transaction = Transaction.Record(_card, category, type, amount, Date, "Test", null);
        _transactions.Add(transaction);

        return transaction;
    }

    [Fact]
    public async Task HandleAsync_NoTransactions_MapsAccountWithOpeningBalance()
    {
        var response = await GetAsync();

        Assert.Equal(_card.Id.Value, response.Id);
        Assert.Equal("Visa", response.Name);
        Assert.Equal(AccountType.CreditCard, response.Type);
        Assert.Equal("COP", response.Currency);
        Assert.Equal(-250_000m, response.OpeningBalance);
        Assert.Equal(-250_000m, response.Balance);
        Assert.Empty(response.Pending);
        Assert.False(response.IsArchived);
    }

    [Fact]
    public async Task HandleAsync_SettledTransactions_AddsThemToBalance()
    {
        Record(TransactionType.Income, Money.Of(1_000_000m, Currency.Cop));
        Record(TransactionType.Expense, Money.Of(85_000m, Currency.Cop));
        Record(TransactionType.Expense, Money.Of(20m, Currency.Usd)).Settle(_card, 4000m); // 80,000 COP

        var response = await GetAsync();

        Assert.Equal(-250_000m + 1_000_000m - 85_000m - 80_000m, response.Balance);
        Assert.Empty(response.Pending);
    }

    [Fact]
    public async Task HandleAsync_PendingTransactions_ListsThemPerCurrencyOutsideBalance()
    {
        Record(TransactionType.Expense, Money.Of(50m, Currency.Usd));
        Record(TransactionType.Income, Money.Of(5m, Currency.Usd));

        var response = await GetAsync();

        Assert.Equal(-250_000m, response.Balance);
        Assert.Equal([new PendingAmountResponse("USD", -45m)], response.Pending);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccount_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new GetAccountQuery(Guid.NewGuid(), _card.Id.Value), TestContext.Current.CancellationToken));
}