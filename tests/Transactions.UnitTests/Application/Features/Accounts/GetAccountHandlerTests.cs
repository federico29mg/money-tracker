using Transactions.Application.Common;
using Transactions.Application.Features.Accounts;
using Transactions.Application.Features.Accounts.GetAccount;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Accounts;

public class GetAccountHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _account =
        Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Of(-1_500.50m, Currency.Usd));

    private readonly GetAccountHandler _handler;
    private readonly InMemoryAccountRepository _repository = new();

    public GetAccountHandlerTests()
    {
        _repository.Add(_account);
        _handler = new GetAccountHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_OwnAccount_ReturnsMappedResponse()
    {
        var response = await _handler.HandleAsync(
            new GetAccountQuery(Owner.Value, _account.Id.Value), TestContext.Current.CancellationToken);

        Assert.Equal(
            new AccountResponse(_account.Id.Value, "Visa", AccountType.CreditCard, "USD", -1_500.50m, false),
            response);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccount_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new GetAccountQuery(Guid.NewGuid(), _account.Id.Value), TestContext.Current.CancellationToken));
}