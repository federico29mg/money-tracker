using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.UnitTests.Domain.AccountAggregate;

public class AccountTests
{
    private static readonly UserId UserId = UserId.From(Guid.NewGuid());

    private static Account OpenAccount(string name = "Visa Gold", AccountType type = AccountType.CreditCard) =>
        Account.Open(UserId, name, type, Money.Zero(Currency.Cop));

    [Fact]
    public void Open_ValidData_SetsProperties()
    {
        var openingBalance = Money.Of(-250_000m, Currency.Cop);

        var account = Account.Open(UserId, "Visa Gold", AccountType.CreditCard, openingBalance);

        Assert.NotEqual(default, account.Id);
        Assert.Equal(UserId, account.UserId);
        Assert.Equal("Visa Gold", account.Name);
        Assert.Equal(AccountType.CreditCard, account.Type);
        Assert.Equal(openingBalance, account.OpeningBalance);
        Assert.Equal(Currency.Cop, account.Currency);
        Assert.False(account.IsArchived);
    }

    [Fact]
    public void Open_NameWithSpaces_TrimsName() => Assert.Equal("Savings", OpenAccount("  Savings  ").Name);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Open_MissingName_ThrowsDomainException(string name) =>
        Assert.Throws<DomainException>(() => OpenAccount(name));

    [Fact]
    public void Open_TooLongName_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => OpenAccount(new string('a', Account.MaxNameLength + 1)));

    [Fact]
    public void Open_UnknownType_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => OpenAccount(type: (AccountType)99));

    [Fact]
    public void Open_DefaultUserId_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() =>
            Account.Open(default, "Cash", AccountType.Cash, Money.Zero(Currency.Cop)));

    [Fact]
    public void Rename_InvalidName_KeepsPreviousName()
    {
        var account = OpenAccount();

        Assert.Throws<DomainException>(() => account.Rename(" "));

        Assert.Equal("Visa Gold", account.Name);
    }

    [Fact]
    public void Archive_MarksAccountAsArchived()
    {
        var account = OpenAccount();

        account.Archive();

        Assert.True(account.IsArchived);
    }
}