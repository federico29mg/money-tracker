using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.UnitTests.Domain.TransactionAggregate;

public class TransactionTests
{
    private const string Name = "Supermarket";
    private static readonly UserId UserId = UserId.From(Guid.NewGuid());
    private static readonly DateOnly Date = new(2026, 10, 19);

    private readonly Account _copCard = Account.Open(UserId, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly Category _groceries = Category.Create(UserId, "Groceries", TransactionType.Expense);

    private Transaction RecordExpense(Money amount, string name = Name, string? description = null) =>
        Transaction.Record(_copCard, _groceries, TransactionType.Expense, amount, Date, name, description);

    [Fact]
    public void Record_ValidData_SetsProperties()
    {
        var amount = Money.Of(85_000m, Currency.Cop);

        var transaction = RecordExpense(amount, description: "Weekly shopping");

        Assert.NotEqual(default, transaction.Id);
        Assert.Equal(UserId, transaction.UserId);
        Assert.Equal(_copCard.Id, transaction.AccountId);
        Assert.Equal(_groceries.Id, transaction.CategoryId);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(Date, transaction.Date);
        Assert.Equal(Name, transaction.Name);
        Assert.Equal("Weekly shopping", transaction.Description);
    }

    [Fact]
    public void Record_AccountCurrency_IsSettledWithSameAmount()
    {
        var amount = Money.Of(85_000m, Currency.Cop);

        var transaction = RecordExpense(amount);

        Assert.True(transaction.IsSettled);
        Assert.Equal(1m, transaction.ExchangeRate);
        Assert.Equal(amount, transaction.SettledAmount);
    }

    [Fact]
    public void Record_ForeignCurrency_IsNotSettled()
    {
        var transaction = RecordExpense(Money.Of(50m, Currency.Usd));

        Assert.False(transaction.IsSettled);
        Assert.Null(transaction.ExchangeRate);
        Assert.Null(transaction.SettledAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Record_NotPositiveAmount_ThrowsDomainException(decimal amount) =>
        Assert.Throws<DomainException>(() => RecordExpense(Money.Of(amount, Currency.Cop)));

    [Fact]
    public void Record_CategoryOfOtherType_ThrowsDomainException()
    {
        var salary = Category.Create(UserId, "Salary", TransactionType.Income);

        Assert.Throws<DomainException>(() =>
            Transaction.Record(_copCard, salary, TransactionType.Expense, Money.Of(1m, Currency.Cop), Date, Name,
                null));
    }

    [Fact]
    public void Record_CategoryOfOtherUser_ThrowsDomainException()
    {
        var otherUsersCategory = Category.Create(UserId.From(Guid.NewGuid()), "Groceries", TransactionType.Expense);

        Assert.Throws<DomainException>(() =>
            Transaction.Record(_copCard, otherUsersCategory, TransactionType.Expense, Money.Of(1m, Currency.Cop),
                Date, Name, null));
    }

    [Fact]
    public void Record_ArchivedAccount_ThrowsDomainException()
    {
        _copCard.Archive();

        Assert.Throws<DomainException>(() => RecordExpense(Money.Of(1m, Currency.Cop)));
    }

    [Fact]
    public void Record_ArchivedCategory_ThrowsDomainException()
    {
        _groceries.Archive();

        Assert.Throws<DomainException>(() => RecordExpense(Money.Of(1m, Currency.Cop)));
    }

    [Fact]
    public void Record_DefaultDate_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Transaction.Record(_copCard, _groceries, TransactionType.Expense, Money.Of(1m, Currency.Cop), default,
                Name, null));

    [Fact]
    public void Record_NameWithSpaces_TrimsName() =>
        Assert.Equal("Netflix", RecordExpense(Money.Of(1m, Currency.Cop), "  Netflix  ").Name);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Record_MissingName_ThrowsDomainException(string name) =>
        Assert.Throws<DomainException>(() => RecordExpense(Money.Of(1m, Currency.Cop), name));

    [Fact]
    public void Record_TooLongName_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            RecordExpense(Money.Of(1m, Currency.Cop), new string('a', Transaction.MaxNameLength + 1)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Record_BlankDescription_StoresNull(string? description) =>
        Assert.Null(RecordExpense(Money.Of(1m, Currency.Cop), description: description).Description);

    [Fact]
    public void Record_TooLongDescription_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            RecordExpense(Money.Of(1m, Currency.Cop),
                description: new string('a', Transaction.MaxDescriptionLength + 1)));

    [Fact]
    public void Settle_ForeignCurrency_StoresSettledAmount()
    {
        var transaction = RecordExpense(Money.Of(50m, Currency.Usd));

        transaction.Settle(_copCard, 4103.25m);

        Assert.True(transaction.IsSettled);
        Assert.Equal(4103.25m, transaction.ExchangeRate);
        Assert.Equal(Money.Of(205_162.50m, Currency.Cop), transaction.SettledAmount);
        Assert.Equal(Money.Of(50m, Currency.Usd), transaction.Amount);
    }

    [Fact]
    public void Settle_ConversionWithManyDecimals_RoundsToCents()
    {
        var transaction = RecordExpense(Money.Of(33.33m, Currency.Usd));

        transaction.Settle(_copCard, 4103.257m); // 136,761.55581

        Assert.Equal(Money.Of(136_761.56m, Currency.Cop), transaction.SettledAmount);
    }

    [Fact]
    public void Settle_ConversionExactlyHalfACent_RoundsAwayFromZero()
    {
        var transaction = RecordExpense(Money.Of(10.25m, Currency.Usd));

        transaction.Settle(_copCard, 0.5m); // 5.125

        Assert.Equal(Money.Of(5.13m, Currency.Cop), transaction.SettledAmount);
    }

    [Fact]
    public void Settle_CalledAgain_CorrectsRateAndAmount()
    {
        var transaction = RecordExpense(Money.Of(50m, Currency.Usd));
        transaction.Settle(_copCard, 4103.25m);

        transaction.Settle(_copCard, 3970m);

        Assert.Equal(3970m, transaction.ExchangeRate);
        Assert.Equal(Money.Of(198_500m, Currency.Cop), transaction.SettledAmount);
    }

    [Fact]
    public void Settle_AccountCurrencyTransaction_ThrowsDomainException()
    {
        var transaction = RecordExpense(Money.Of(85_000m, Currency.Cop));

        Assert.Throws<DomainException>(() => transaction.Settle(_copCard, 1.05m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4103.25)]
    public void Settle_NotPositiveRate_ThrowsDomainException(decimal exchangeRate)
    {
        var transaction = RecordExpense(Money.Of(50m, Currency.Usd));

        Assert.Throws<DomainException>(() => transaction.Settle(_copCard, exchangeRate));
    }

    [Fact]
    public void Settle_RateWithTooManyDecimals_ThrowsDomainException()
    {
        var transaction = RecordExpense(Money.Of(50m, Currency.Usd));

        Assert.Throws<DomainException>(() => transaction.Settle(_copCard, 4103.123456789m));
    }

    [Fact]
    public void Settle_InvalidRate_KeepsPendingState()
    {
        var transaction = RecordExpense(Money.Of(50m, Currency.Usd));

        Assert.Throws<DomainException>(() => transaction.Settle(_copCard, 0m));

        Assert.False(transaction.IsSettled);
        Assert.Null(transaction.ExchangeRate);
    }

    [Fact]
    public void Settle_OtherAccount_ThrowsArgumentException()
    {
        var transaction = RecordExpense(Money.Of(50m, Currency.Usd));
        var otherAccount = Account.Open(UserId, "Savings", AccountType.Savings, Money.Zero(Currency.Cop));

        Assert.Throws<ArgumentException>(() => transaction.Settle(otherAccount, 4103.25m));
    }
}