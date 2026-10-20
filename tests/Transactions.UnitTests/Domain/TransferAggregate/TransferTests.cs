using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.UnitTests.Domain.TransferAggregate;

public class TransferTests
{
    private static readonly UserId UserId = UserId.From(Guid.NewGuid());
    private static readonly DateOnly Date = new(2026, 10, 20);

    private readonly Account _card = Account.Open(UserId, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly Account _cashUsd = Account.Open(UserId, "Cash USD", AccountType.Cash, Money.Zero(Currency.Usd));

    private readonly Account _checking =
        Account.Open(UserId, "Checking", AccountType.Checking, Money.Zero(Currency.Cop));

    private Transfer Pay(Money amount, Money? received = null, string? description = null) =>
        Transfer.Record(_checking, _card, amount, received, Date, description);

    [Fact]
    public void Record_SameCurrency_ReceivesTheSameAmount()
    {
        var amount = Money.Of(1_231_000m, Currency.Cop);

        var transfer = Pay(amount, description: "Card payment");

        Assert.NotEqual(default, transfer.Id);
        Assert.Equal(UserId, transfer.UserId);
        Assert.Equal(_checking.Id, transfer.FromAccountId);
        Assert.Equal(_card.Id, transfer.ToAccountId);
        Assert.Equal(amount, transfer.Amount);
        Assert.Equal(amount, transfer.ReceivedAmount);
        Assert.Equal(Date, transfer.Date);
        Assert.Equal("Card payment", transfer.Description);
    }

    [Fact]
    public void Record_SameCurrencyWithMatchingReceivedAmount_IsAccepted()
    {
        var amount = Money.Of(100m, Currency.Cop);

        Assert.Equal(amount, Pay(amount, amount).ReceivedAmount);
    }

    [Fact]
    public void Record_SameCurrencyWithDifferentReceivedAmount_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => Pay(Money.Of(100m, Currency.Cop), Money.Of(90m, Currency.Cop)));

    [Fact]
    public void Record_DifferentCurrencies_StoresBothAmounts()
    {
        var transfer = Transfer.Record(
            _checking, _cashUsd, Money.Of(500_000m, Currency.Cop), Money.Of(120m, Currency.Usd), Date, null);

        Assert.Equal(Money.Of(500_000m, Currency.Cop), transfer.Amount);
        Assert.Equal(Money.Of(120m, Currency.Usd), transfer.ReceivedAmount);
    }

    [Fact]
    public void Record_DifferentCurrenciesWithoutReceivedAmount_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Transfer.Record(_checking, _cashUsd, Money.Of(500_000m, Currency.Cop), null, Date, null));

    [Fact]
    public void Record_ReceivedAmountInWrongCurrency_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Transfer.Record(
                _checking, _cashUsd, Money.Of(500_000m, Currency.Cop), Money.Of(500_000m, Currency.Cop), Date, null));

    [Fact]
    public void Record_NotPositiveReceivedAmount_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Transfer.Record(_checking, _cashUsd, Money.Of(500_000m, Currency.Cop), Money.Zero(Currency.Usd), Date,
                null));

    [Fact]
    public void Record_AmountNotInSourceCurrency_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => Pay(Money.Of(100m, Currency.Usd)));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Record_NotPositiveAmount_ThrowsDomainException(decimal amount) =>
        Assert.Throws<DomainException>(() => Pay(Money.Of(amount, Currency.Cop)));

    [Fact]
    public void Record_SameAccount_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Transfer.Record(_checking, _checking, Money.Of(1m, Currency.Cop), null, Date, null));

    [Fact]
    public void Record_AccountsOfDifferentUsers_ThrowsDomainException()
    {
        var someoneElses =
            Account.Open(UserId.From(Guid.NewGuid()), "Theirs", AccountType.Cash, Money.Zero(Currency.Cop));

        Assert.Throws<DomainException>(() =>
            Transfer.Record(_checking, someoneElses, Money.Of(1m, Currency.Cop), null, Date, null));
    }

    [Fact]
    public void Record_ArchivedSourceAccount_ThrowsDomainException()
    {
        _checking.Archive();

        Assert.Throws<DomainException>(() => Pay(Money.Of(1m, Currency.Cop)));
    }

    [Fact]
    public void Record_ArchivedDestinationAccount_ThrowsDomainException()
    {
        _card.Archive();

        Assert.Throws<DomainException>(() => Pay(Money.Of(1m, Currency.Cop)));
    }

    [Fact]
    public void Record_DefaultDate_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Transfer.Record(_checking, _card, Money.Of(1m, Currency.Cop), null, default, null));

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Record_BlankDescription_StoresNull(string? description) =>
        Assert.Null(Pay(Money.Of(1m, Currency.Cop), description: description).Description);

    [Fact]
    public void Record_TooLongDescription_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Pay(Money.Of(1m, Currency.Cop), description: new string('a', Transfer.MaxDescriptionLength + 1)));
}