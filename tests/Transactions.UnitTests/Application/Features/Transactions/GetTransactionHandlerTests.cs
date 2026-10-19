using Transactions.Application.Common;
using Transactions.Application.Features.Transactions;
using Transactions.Application.Features.Transactions.GetTransaction;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Transactions;

public class GetTransactionHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _copCard = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly GetTransactionHandler _handler;
    private readonly InMemoryTransactionRepository _repository = new();
    private readonly Transaction _transaction;

    public GetTransactionHandlerTests()
    {
        var subscriptions = Category.Create(Owner, "Subscriptions", TransactionType.Expense);
        _transaction = Transaction.Record(
            _copCard, subscriptions, TransactionType.Expense, Money.Of(20m, Currency.Usd), new DateOnly(2026, 10, 1),
            "Netflix", null);
        _transaction.Settle(_copCard, 4000m);

        _repository.Add(_transaction);
        _handler = new GetTransactionHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_OwnTransaction_ReturnsMappedResponse()
    {
        var response = await _handler.HandleAsync(
            new GetTransactionQuery(Owner.Value, _transaction.Id.Value), TestContext.Current.CancellationToken);

        Assert.Equal(
            new TransactionResponse(
                _transaction.Id.Value,
                _copCard.Id.Value,
                _transaction.CategoryId.Value,
                TransactionType.Expense,
                new DateOnly(2026, 10, 1),
                "Netflix",
                null,
                20m,
                "USD",
                4000m,
                80_000m,
                "COP",
                true),
            response);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersTransaction_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new GetTransactionQuery(Guid.NewGuid(), _transaction.Id.Value),
                TestContext.Current.CancellationToken));
}