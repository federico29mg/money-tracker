using Transactions.Application.Common;
using Transactions.Application.Features.Transactions.SettleTransactions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transactions;

public class SettleTransactionsHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly InMemoryAccountRepository _accounts = new();
    private readonly Account _card = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly SettleTransactionsHandler _handler;
    private readonly InMemoryTransactionRepository _transactions = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public SettleTransactionsHandlerTests()
    {
        _accounts.Add(_card);
        _handler = new SettleTransactionsHandler(_accounts, _transactions, _unitOfWork);
    }

    private Transaction Record(Money amount, Account? account = null)
    {
        var category = Category.Create(Owner, "Shopping", TransactionType.Expense);
        var transaction = Transaction.Record(
            account ?? _card, category, TransactionType.Expense, amount, new DateOnly(2026, 10, 19), "Purchase", null);
        _transactions.Add(transaction);

        return transaction;
    }

    private Task<SettlementResponse> SettleAsync(
        IEnumerable<Transaction> transactions,
        decimal? exchangeRate = null,
        decimal? totalPaid = null,
        Guid? userId = null) =>
        _handler.HandleAsync(
            new SettleTransactionsCommand(
                userId ?? Owner.Value,
                _card.Id.Value,
                [.. transactions.Select(transaction => transaction.Id.Value)],
                exchangeRate,
                totalPaid),
            TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_ExchangeRate_SettlesAllTransactionsAndSavesOnce()
    {
        var amazon = Record(Money.Of(50m, Currency.Usd));
        var netflix = Record(Money.Of(20m, Currency.Usd));

        var response = await SettleAsync([amazon, netflix], exchangeRate: 4000m);

        Assert.Equal(Money.Of(200_000m, Currency.Cop), amazon.SettledAmount);
        Assert.Equal(Money.Of(80_000m, Currency.Cop), netflix.SettledAmount);
        Assert.Equal(new SettlementResponse(2, "USD", 4000m, "COP", 280_000m), response);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_TotalPaid_DerivesRateFromTotal()
    {
        var amazon = Record(Money.Of(50m, Currency.Usd));
        var netflix = Record(Money.Of(20m, Currency.Usd));

        var response = await SettleAsync([amazon, netflix], totalPaid: 287_000m);

        Assert.Equal(4100m, response.ExchangeRate);
        Assert.Equal(287_000m, response.TotalSettled);
    }

    [Fact]
    public async Task HandleAsync_TotalPaidWithRounding_SettledTotalCanDifferByCents()
    {
        var purchases = Enumerable.Range(0, 3).Select(_ => Record(Money.Of(33.33m, Currency.Usd))).ToList();

        var response = await SettleAsync(purchases, totalPaid: 410_000m);

        Assert.Equal(4100.41004100m, response.ExchangeRate);
        Assert.Equal(410_000.01m, response.TotalSettled);
    }

    [Fact]
    public async Task HandleAsync_AlreadySettledTransaction_CorrectsIt()
    {
        var amazon = Record(Money.Of(50m, Currency.Usd));
        amazon.Settle(_card, 4000m);

        await SettleAsync([amazon], exchangeRate: 3900m);

        Assert.Equal(Money.Of(195_000m, Currency.Cop), amazon.SettledAmount);
    }

    [Fact]
    public async Task HandleAsync_NeitherRateNorTotal_ThrowsValidationException() =>
        await Assert.ThrowsAsync<ValidationException>(() => SettleAsync([Record(Money.Of(50m, Currency.Usd))]));

    [Fact]
    public async Task HandleAsync_BothRateAndTotal_ThrowsValidationException() =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            SettleAsync([Record(Money.Of(50m, Currency.Usd))], exchangeRate: 4000m, totalPaid: 200_000m));

    [Fact]
    public async Task HandleAsync_ZeroTotalPaid_ThrowsValidationException() =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            SettleAsync([Record(Money.Of(50m, Currency.Usd))], totalPaid: 0m));

    [Fact]
    public async Task HandleAsync_NoTransactions_ThrowsValidationException() =>
        await Assert.ThrowsAsync<ValidationException>(() => SettleAsync([], exchangeRate: 4000m));

    [Fact]
    public async Task HandleAsync_UnknownTransaction_ThrowsNotFoundAndSavesNothing()
    {
        var amazon = Record(Money.Of(50m, Currency.Usd));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new SettleTransactionsCommand(Owner.Value, _card.Id.Value, [amazon.Id.Value, Guid.NewGuid()], 4000m,
                    null),
                TestContext.Current.CancellationToken));

        Assert.False(amazon.IsSettled);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_TransactionOfOtherAccount_ThrowsValidationException()
    {
        var otherCard = Account.Open(Owner, "Mastercard", AccountType.CreditCard, Money.Zero(Currency.Cop));
        _accounts.Add(otherCard);

        await Assert.ThrowsAsync<ValidationException>(() =>
            SettleAsync([Record(Money.Of(50m, Currency.Usd), otherCard)], exchangeRate: 4000m));
    }

    [Fact]
    public async Task HandleAsync_AccountCurrencyTransaction_ThrowsValidationException() =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            SettleAsync([Record(Money.Of(85_000m, Currency.Cop))], exchangeRate: 1m));

    [Fact]
    public async Task HandleAsync_RateWithTooManyDecimals_ThrowsDomainExceptionAndSavesNothing()
    {
        var amazon = Record(Money.Of(50m, Currency.Usd));

        await Assert.ThrowsAsync<DomainException>(() => SettleAsync([amazon], exchangeRate: 4000.123456789m));

        Assert.False(amazon.IsSettled);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccount_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            SettleAsync([Record(Money.Of(50m, Currency.Usd))], exchangeRate: 4000m, userId: Guid.NewGuid()));
}