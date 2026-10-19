using Transactions.Application.Common;
using Transactions.Application.Features.Transactions.RecordTransaction;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transactions;

public class RecordTransactionHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());
    private static readonly DateOnly Date = new(2026, 10, 19);

    private readonly InMemoryAccountRepository _accounts = new();
    private readonly InMemoryCategoryRepository _categories = new();
    private readonly Account _copCard = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly Category _groceries = Category.Create(Owner, "Groceries", TransactionType.Expense);
    private readonly RecordTransactionHandler _handler;
    private readonly InMemoryTransactionRepository _transactions = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public RecordTransactionHandlerTests()
    {
        _accounts.Add(_copCard);
        _categories.Add(_groceries);
        _handler = new RecordTransactionHandler(_accounts, _categories, _transactions, _unitOfWork);
    }

    private RecordTransactionCommand Command(
        decimal amount = 85_000m,
        string? currencyCode = null,
        Guid? userId = null,
        Guid? accountId = null,
        Guid? categoryId = null) =>
        new(
            userId ?? Owner.Value,
            accountId ?? _copCard.Id.Value,
            categoryId ?? _groceries.Id.Value,
            TransactionType.Expense,
            amount,
            currencyCode,
            Date,
            "Supermarket",
            "Weekly shopping");

    [Fact]
    public async Task HandleAsync_ValidCommand_StoresTransactionAndReturnsId()
    {
        var id = await _handler.HandleAsync(Command(), TestContext.Current.CancellationToken);

        var transaction = Assert.Single(_transactions.StoredTransactions);
        Assert.Equal(id, transaction.Id.Value);
        Assert.Equal(_copCard.Id, transaction.AccountId);
        Assert.Equal(_groceries.Id, transaction.CategoryId);
        Assert.Equal("Supermarket", transaction.Name);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_NoCurrency_UsesAccountCurrencyAndSettles()
    {
        await _handler.HandleAsync(Command(currencyCode: null), TestContext.Current.CancellationToken);

        var transaction = Assert.Single(_transactions.StoredTransactions);
        Assert.Equal(Money.Of(85_000m, Currency.Cop), transaction.Amount);
        Assert.True(transaction.IsSettled);
    }

    [Fact]
    public async Task HandleAsync_ForeignCurrency_StoresPendingTransaction()
    {
        await _handler.HandleAsync(Command(50m, "usd"), TestContext.Current.CancellationToken);

        var transaction = Assert.Single(_transactions.StoredTransactions);
        Assert.Equal(Money.Of(50m, Currency.Usd), transaction.Amount);
        Assert.False(transaction.IsSettled);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccount_ThrowsNotFoundAndSavesNothing()
    {
        var othersAccount = Account.Open(
            UserId.From(Guid.NewGuid()), "Theirs", AccountType.Cash, Money.Zero(Currency.Cop));
        _accounts.Add(othersAccount);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(Command(accountId: othersAccount.Id.Value), TestContext.Current.CancellationToken));

        Assert.Empty(_transactions.StoredTransactions);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UnknownCategory_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(Command(categoryId: Guid.NewGuid()), TestContext.Current.CancellationToken));

    [Fact]
    public async Task HandleAsync_ArchivedCategory_ThrowsDomainExceptionAndSavesNothing()
    {
        _groceries.Archive();

        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(Command(), TestContext.Current.CancellationToken));

        Assert.Empty(_transactions.StoredTransactions);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}