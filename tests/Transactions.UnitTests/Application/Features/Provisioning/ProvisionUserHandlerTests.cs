using Transactions.Application.Features.Provisioning.ProvisionUser;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Provisioning;

public class ProvisionUserHandlerTests
{
    private static readonly UserId NewUser = UserId.From(Guid.NewGuid());

    private readonly InMemoryAccountRepository _accounts = new();
    private readonly InMemoryCategoryRepository _categories = new();
    private readonly ProvisionUserHandler _handler;
    private readonly FakeUnitOfWork _unitOfWork = new();

    public ProvisionUserHandlerTests() => _handler = new ProvisionUserHandler(_accounts, _categories, _unitOfWork);

    private Task ProvisionAsync(string currencyCode = "COP") =>
        _handler.HandleAsync(
            new ProvisionUserCommand(NewUser.Value, currencyCode), TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_NewUser_CreatesCashAccountAndDefaultCategories()
    {
        await ProvisionAsync();

        var cash = Assert.Single(_accounts.StoredAccounts);
        Assert.Equal(NewUser, cash.UserId);
        Assert.Equal(ProvisionUserHandler.CashAccountName, cash.Name);
        Assert.Equal(AccountType.Cash, cash.Type);
        Assert.Equal(Money.Zero(Currency.Cop), cash.OpeningBalance);

        Assert.Equal(2, _categories.StoredCategories.Count);
        Assert.All(_categories.StoredCategories, category =>
        {
            Assert.Equal(Category.DefaultName, category.Name);
            Assert.True(category.IsDefault);
        });
        Assert.Contains(_categories.StoredCategories, category => category.Type == TransactionType.Income);
        Assert.Contains(_categories.StoredCategories, category => category.Type == TransactionType.Expense);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_RunTwice_CreatesNothingTheSecondTime()
    {
        await ProvisionAsync();
        await ProvisionAsync();

        Assert.Single(_accounts.StoredAccounts);
        Assert.Equal(2, _categories.StoredCategories.Count);
    }

    [Fact]
    public async Task HandleAsync_ArchivedCashAccount_IsNotRecreated()
    {
        var cash = Account.Open(NewUser, "My cash", AccountType.Cash, Money.Zero(Currency.Cop));
        cash.Archive();
        _accounts.Add(cash);

        await ProvisionAsync();

        Assert.Single(_accounts.StoredAccounts);
    }

    [Fact]
    public async Task HandleAsync_CashInAnotherCurrency_CreatesCashInTheUsersCurrency()
    {
        _accounts.Add(Account.Open(NewUser, "Dollars", AccountType.Cash, Money.Zero(Currency.Usd)));

        await ProvisionAsync();

        Assert.Contains(_accounts.StoredAccounts, account => account.Currency == Currency.Cop);
        Assert.Equal(2, _accounts.StoredAccounts.Count);
    }

    [Fact]
    public async Task HandleAsync_ExistingUncategorizedCategory_BecomesTheDefault()
    {
        var existing = Category.Create(NewUser, "uncategorized", TransactionType.Expense);
        existing.Archive();
        _categories.Add(existing);

        await ProvisionAsync();

        Assert.True(existing.IsDefault);
        Assert.False(existing.IsArchived);
        Assert.Single(_categories.StoredCategories, category => category.Type == TransactionType.Expense);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedCurrency_ThrowsAndSavesNothing()
    {
        await Assert.ThrowsAsync<DomainException>(() => ProvisionAsync("XYZ"));

        Assert.Empty(_accounts.StoredAccounts);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}