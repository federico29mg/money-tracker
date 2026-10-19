using Transactions.Application.Common;
using Transactions.Application.Features.Accounts.RenameAccount;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Accounts;

public class RenameAccountHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _account = Account.Open(Owner, "Visa", AccountType.CreditCard, Money.Zero(Currency.Cop));
    private readonly RenameAccountHandler _handler;
    private readonly InMemoryAccountRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public RenameAccountHandlerTests()
    {
        _repository.Add(_account);
        _handler = new RenameAccountHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task HandleAsync_OwnAccount_RenamesAndSaves()
    {
        await _handler.HandleAsync(
            new RenameAccountCommand(Owner.Value, _account.Id.Value, "Visa Gold"),
            TestContext.Current.CancellationToken);

        Assert.Equal("Visa Gold", _account.Name);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccount_ThrowsNotFoundAndChangesNothing()
    {
        var command = new RenameAccountCommand(Guid.NewGuid(), _account.Id.Value, "Hacked");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal("Visa", _account.Name);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UnknownAccount_ThrowsNotFoundException() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new RenameAccountCommand(Owner.Value, Guid.NewGuid(), "Savings"),
                TestContext.Current.CancellationToken));
}