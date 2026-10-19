using Transactions.Domain.Common;

namespace Transactions.UnitTests.Domain.Common;

public class MoneyTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(10.5)]
    [InlineData(10.55)]
    [InlineData(-10.55)]
    [InlineData(0)]
    public void Of_AtMostTwoDecimals_CreatesMoney(decimal amount)
    {
        var money = Money.Of(amount, Currency.Usd);

        Assert.Equal(amount, money.Amount);
        Assert.Equal(Currency.Usd, money.Currency);
    }

    [Fact]
    public void Of_MoreThanTwoDecimals_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => Money.Of(10.555m, Currency.Usd));

    [Fact]
    public void Of_SameAmountAndCurrency_AreEqual() =>
        Assert.Equal(Money.Of(10m, Currency.Cop), Money.Of(10.00m, Currency.Cop));

    [Fact]
    public void Of_SameAmountDifferentCurrency_AreNotEqual() =>
        Assert.NotEqual(Money.Of(10m, Currency.Cop), Money.Of(10m, Currency.Usd));

    [Theory]
    [InlineData(0.01, true)]
    [InlineData(0, false)]
    [InlineData(-0.01, false)]
    public void IsPositive_ReturnsWhetherAmountIsAboveZero(decimal amount, bool expected) =>
        Assert.Equal(expected, Money.Of(amount, Currency.Usd).IsPositive);
}