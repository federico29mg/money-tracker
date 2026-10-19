using Transactions.Domain.Common;

namespace Transactions.UnitTests.Domain.Common;

public class CurrencyTests
{
    [Theory]
    [InlineData("USD")]
    [InlineData("usd")]
    [InlineData(" Usd ")]
    public void FromCode_SupportedCodeInAnyFormat_ReturnsUsd(string code)
    {
        var currency = Currency.FromCode(code);

        Assert.Same(Currency.Usd, currency);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FromCode_UnsupportedOrMissingCode_ThrowsDomainException(string? code) =>
        Assert.Throws<DomainException>(() => Currency.FromCode(code));
}