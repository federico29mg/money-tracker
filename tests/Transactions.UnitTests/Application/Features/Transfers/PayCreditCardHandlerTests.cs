using Transactions.Application.Common;
using Transactions.Application.Features.Transfers.PayCreditCard;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transfers;

public class PayCreditCardHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());
    private static readonly DateOnly Date = new(2026, 10, 21);

    private readonly InMemoryAccountRepository _accounts = new();
    private readonly Account _card = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));

    private readonly Account _checking =
        Account.Open(Owner, "Checking", AccountType.Checking, Money.Zero(Currency.Cop));

    private readonly PayCreditCardHandler _handler;
    private readonly InMemoryTransactionRepository _transactions = new();
    private readonly InMemoryTransferRepository _transfers = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public PayCreditCardHandlerTests()
    {
        _accounts.Add(_card);
        _accounts.Add(_checking);
        _handler = new PayCreditCardHandler(_accounts, _transactions, _transfers, _unitOfWork);
    }

    private Transaction Charge(Money amount, Account? account = null)
    {
        var category = Category.Create(Owner, "Shopping", TransactionType.Expense);
        var transaction = Transaction.Record(
            account ?? _card, category, TransactionType.Expense, amount, new DateOnly(2026, 10, 10), "Purchase", null);
        _transactions.Add(transaction);

        return transaction;
    }

    private Task<PayCreditCardResponse> PayAsync(
        decimal amount,
        PaymentSettlement? settlement = null,
        Guid? cardId = null,
        string? description = null,
        Guid? userId = null) =>
        _handler.HandleAsync(
            new PayCreditCardCommand(
                userId ?? Owner.Value,
                cardId ?? _card.Id.Value,
                _checking.Id.Value,
                amount,
                null,
                Date,
                description,
                settlement),
            TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_WithoutSettlement_RecordsTransferToTheCard()
    {
        var response = await PayAsync(1_231_000m);

        var transfer = Assert.Single(_transfers.StoredTransfers);
        Assert.Equal(response.TransferId, transfer.Id.Value);
        Assert.Equal(_checking.Id, transfer.FromAccountId);
        Assert.Equal(_card.Id, transfer.ToAccountId);
        Assert.Equal(Money.Of(1_231_000m, Currency.Cop), transfer.Amount);
        Assert.Equal("Card payment", transfer.Description);
        Assert.Null(response.Settlement);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithDescription_KeepsIt()
    {
        await PayAsync(100m, description: "October statement");

        Assert.Equal("October statement", Assert.Single(_transfers.StoredTransfers).Description);
    }

    [Fact]
    public async Task HandleAsync_WithSettlement_SettlesChargesAndSavesOnce()
    {
        var amazon = Charge(Money.Of(50m, Currency.Usd));
        var netflix = Charge(Money.Of(20m, Currency.Usd));

        var response = await PayAsync(
            1_000_000m, new PaymentSettlement([amazon.Id.Value, netflix.Id.Value], 4100m, null));

        Assert.Single(_transfers.StoredTransfers);
        Assert.Equal(Money.Of(205_000m, Currency.Cop), amazon.SettledAmount);
        Assert.Equal(Money.Of(82_000m, Currency.Cop), netflix.SettledAmount);
        Assert.Equal(287_000m, response.Settlement?.TotalSettled);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_InvalidSettlement_RecordsNothing()
    {
        var otherCard = Account.Open(Owner, "Mastercard", AccountType.CreditCard, Money.Zero(Currency.Cop));
        var chargeOnOtherCard = Charge(Money.Of(50m, Currency.Usd), otherCard);

        await Assert.ThrowsAsync<ValidationException>(() =>
            PayAsync(1_000_000m, new PaymentSettlement([chargeOnOtherCard.Id.Value], 4100m, null)));

        Assert.Empty(_transfers.StoredTransfers);
        Assert.False(chargeOnOtherCard.IsSettled);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_DestinationIsNotACreditCard_ThrowsValidationException()
    {
        var savings = Account.Open(Owner, "Savings", AccountType.Savings, Money.Zero(Currency.Cop));
        _accounts.Add(savings);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => PayAsync(100m, cardId: savings.Id.Value));

        Assert.Contains(nameof(PayCreditCardCommand.CardAccountId), exception.Errors.Keys);
        Assert.Empty(_transfers.StoredTransfers);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccounts_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() => PayAsync(100m, userId: Guid.NewGuid()));
}