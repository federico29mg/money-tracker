using Transactions.Application.Common;
using Transactions.Application.Features.Transfers.RecordTransfer;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transfers;

public class RecordTransferHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());
    private static readonly DateOnly Date = new(2026, 10, 20);

    private readonly InMemoryAccountRepository _accounts = new();
    private readonly Account _card = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly Account _cashUsd = Account.Open(Owner, "Cash USD", AccountType.Cash, Money.Zero(Currency.Usd));

    private readonly Account _checking =
        Account.Open(Owner, "Checking", AccountType.Checking, Money.Zero(Currency.Cop));

    private readonly RecordTransferHandler _handler;
    private readonly InMemoryTransferRepository _transfers = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public RecordTransferHandlerTests()
    {
        _accounts.Add(_checking);
        _accounts.Add(_card);
        _accounts.Add(_cashUsd);
        _handler = new RecordTransferHandler(_accounts, _transfers, _unitOfWork);
    }

    private Task<Guid> RecordAsync(Account to, decimal amount, decimal? received = null, Guid? userId = null) =>
        _handler.HandleAsync(
            new RecordTransferCommand(
                userId ?? Owner.Value, _checking.Id.Value, to.Id.Value, amount, received, Date, "Card payment"),
            TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_SameCurrency_StoresTransferInAccountCurrencies()
    {
        var id = await RecordAsync(_card, 1_231_000m);

        var transfer = Assert.Single(_transfers.StoredTransfers);
        Assert.Equal(id, transfer.Id.Value);
        Assert.Equal(Money.Of(1_231_000m, Currency.Cop), transfer.Amount);
        Assert.Equal(Money.Of(1_231_000m, Currency.Cop), transfer.ReceivedAmount);
        Assert.Equal("Card payment", transfer.Description);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_DifferentCurrencies_UsesEachAccountsCurrency()
    {
        await RecordAsync(_cashUsd, 500_000m, 120m);

        var transfer = Assert.Single(_transfers.StoredTransfers);
        Assert.Equal(Money.Of(500_000m, Currency.Cop), transfer.Amount);
        Assert.Equal(Money.Of(120m, Currency.Usd), transfer.ReceivedAmount);
    }

    [Fact]
    public async Task HandleAsync_DifferentCurrenciesWithoutReceivedAmount_ThrowsAndSavesNothing()
    {
        await Assert.ThrowsAsync<DomainException>(() => RecordAsync(_cashUsd, 500_000m));

        Assert.Empty(_transfers.StoredTransfers);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UnknownDestination_ThrowsNotFoundException()
    {
        var notStored = Account.Open(Owner, "Not stored", AccountType.Savings, Money.Zero(Currency.Cop));

        await Assert.ThrowsAsync<NotFoundException>(() => RecordAsync(notStored, 1m));
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccounts_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() => RecordAsync(_card, 1m, userId: Guid.NewGuid()));
}