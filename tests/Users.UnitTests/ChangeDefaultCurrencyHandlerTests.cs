using Users.Application.Common;
using Users.Application.Features.ChangeDefaultCurrency;
using Users.Domain;
using Users.UnitTests.Fakes;

namespace Users.UnitTests;

public class ChangeDefaultCurrencyHandlerTests
{
    private readonly InMemoryUserRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ChangeDefaultCurrencyHandler _handler;

    public ChangeDefaultCurrencyHandlerTests() =>
        _handler = new ChangeDefaultCurrencyHandler(_repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ExistingUser_ChangesCurrencyAndSaves()
    {
        var user = User.Register(UserId.New(), "Ana", UserSettings.Default);
        _repository.Add(user);

        await _handler.HandleAsync(
            new ChangeDefaultCurrencyCommand(user.Id.Value, "USD"), TestContext.Current.CancellationToken);

        Assert.Equal(Currency.Usd, user.Settings.DefaultCurrency);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UnknownUser_ThrowsAndSavesNothing()
    {
        var command = new ChangeDefaultCurrencyCommand(Guid.NewGuid(), "USD");

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.HandleAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}