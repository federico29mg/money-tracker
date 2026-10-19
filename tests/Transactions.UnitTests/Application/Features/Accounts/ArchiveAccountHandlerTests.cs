using Transactions.Application.Common;
using Transactions.Application.Features.Accounts.ArchiveAccount;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.UnitTests.Fakes;

namespace Transactions.UnitTests.Application.Features.Accounts;

public class ArchiveAccountHandlerTests
{
    private static readonly UserId Owner = UserId.From(Guid.NewGuid());

    private readonly Account _account = Account.Open(Owner, "Cash", AccountType.Cash, Money.Zero(Currency.Cop));
    private readonly ArchiveAccountHandler _handler;
    private readonly InMemoryAccountRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public ArchiveAccountHandlerTests()
    {
        _repository.Add(_account);
        _handler = new ArchiveAccountHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task HandleAsync_OwnAccount_ArchivesAndSaves()
    {
        await _handler.HandleAsync(
            new ArchiveAccountCommand(Owner.Value, _account.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(_account.IsArchived);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_OtherUsersAccount_ThrowsNotFoundAndChangesNothing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(
                new ArchiveAccountCommand(Guid.NewGuid(), _account.Id.Value), TestContext.Current.CancellationToken));

        Assert.False(_account.IsArchived);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}