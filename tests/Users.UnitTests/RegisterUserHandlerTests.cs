using Users.Application.Features.RegisterUser;
using Users.Domain.Common;
using Users.Domain.UserAggregate;
using Users.UnitTests.Fakes;

namespace Users.UnitTests;

public class RegisterUserHandlerTests
{
    private readonly RegisterUserHandler _handler;
    private readonly InMemoryUserRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public RegisterUserHandlerTests() => _handler = new RegisterUserHandler(_repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ValidCommand_StoresUserAndReturnsId()
    {
        var command = new RegisterUserCommand("Ana", null);

        var userId = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        var user = Assert.Single(_repository.StoredUsers);
        Assert.Equal(userId, user.Id.Value);
        Assert.Equal(command.DisplayName, user.DisplayName);
    }

    [Fact]
    public async Task HandleAsync_NoCurrency_UsesDefaultSettings()
    {
        var command = new RegisterUserCommand("Ana", null);

        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        var user = Assert.Single(_repository.StoredUsers);
        Assert.Equal(UserSettings.Default, user.Settings);
    }

    [Fact]
    public async Task HandleAsync_WithCurrency_UsesRequestedCurrency()
    {
        var command = new RegisterUserCommand("Ana", "usd");

        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        var user = Assert.Single(_repository.StoredUsers);
        Assert.Equal(Currency.Usd, user.Settings.DefaultCurrency);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_SavesOnce()
    {
        var command = new RegisterUserCommand("Ana", null);

        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedCurrency_ThrowsAndSavesNothing()
    {
        var command = new RegisterUserCommand("Ana", "EUR");

        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(command, TestContext.Current.CancellationToken));

        Assert.Empty(_repository.StoredUsers);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}