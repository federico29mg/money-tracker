using Transactions.Application.Features.Accounts.OpenAccount;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Accounts;

public class OpenAccountHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly OpenAccountHandler _handler;
    private readonly InMemoryAccountRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public OpenAccountHandlerTests() => _handler = new OpenAccountHandler(_repository, _unitOfWork);

    private static OpenAccountCommand Command(string currencyCode = "COP", decimal openingBalance = -250_000m) =>
        new(UserId, "Visa Gold", AccountType.CreditCard, currencyCode, openingBalance);

    [Fact]
    public async Task HandleAsync_ValidCommand_StoresAccountAndReturnsId()
    {
        var id = await _handler.HandleAsync(Command(), TestContext.Current.CancellationToken);

        var account = Assert.Single(_repository.StoredAccounts);
        Assert.Equal(id, account.Id.Value);
        Assert.Equal(UserId, account.UserId.Value);
        Assert.Equal("Visa Gold", account.Name);
        Assert.Equal(AccountType.CreditCard, account.Type);
        Assert.Equal(Money.Of(-250_000m, Currency.Cop), account.OpeningBalance);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedCurrency_ThrowsAndSavesNothing()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(Command(currencyCode: "XYZ"), TestContext.Current.CancellationToken));

        Assert.Empty(_repository.StoredAccounts);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_OpeningBalanceWithTooManyDecimals_ThrowsDomainException() =>
        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(Command(openingBalance: 10.555m), TestContext.Current.CancellationToken));
}