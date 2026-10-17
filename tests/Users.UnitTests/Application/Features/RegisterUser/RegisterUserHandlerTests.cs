using Users.Application.Common;
using Users.Application.Features.RegisterUser;
using Users.Domain.Common;
using Users.Domain.UserAggregate;
using Users.UnitTests.Fakes;

namespace Users.UnitTests.Application.Features.RegisterUser;

public class RegisterUserHandlerTests
{
    private const string Email = "ana@example.com";
    private const string Password = "correct horse battery";

    private readonly RegisterUserHandler _handler;
    private readonly FakeIdentityService _identityService = new();
    private readonly InMemoryUserRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public RegisterUserHandlerTests() =>
        _handler = new RegisterUserHandler(_repository, _identityService, _unitOfWork);

    private static RegisterUserCommand Command(string displayName = "Ana", string? currencyCode = null) =>
        new(Email, Password, displayName, currencyCode);

    [Fact]
    public async Task HandleAsync_ValidCommand_StoresUserAndReturnsId()
    {
        var command = Command();

        var userId = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        var user = Assert.Single(_repository.StoredUsers);
        Assert.Equal(userId, user.Id.Value);
        Assert.Equal(command.DisplayName, user.DisplayName);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesAccountWithSameId()
    {
        var userId = await _handler.HandleAsync(Command(), TestContext.Current.CancellationToken);

        var account = Assert.Single(_identityService.Accounts);
        Assert.Equal(Email, account.Key);
        Assert.Equal(userId, account.Value.UserId.Value);
    }

    [Fact]
    public async Task HandleAsync_NoCurrency_UsesDefaultSettings()
    {
        await _handler.HandleAsync(Command(), TestContext.Current.CancellationToken);

        var user = Assert.Single(_repository.StoredUsers);
        Assert.Equal(UserSettings.Default, user.Settings);
    }

    [Fact]
    public async Task HandleAsync_WithCurrency_UsesRequestedCurrency()
    {
        await _handler.HandleAsync(Command(currencyCode: "usd"), TestContext.Current.CancellationToken);

        var user = Assert.Single(_repository.StoredUsers);
        Assert.Equal(Currency.Usd, user.Settings.DefaultCurrency);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_SavesOnceInOneTransaction()
    {
        await _handler.HandleAsync(Command(), TestContext.Current.CancellationToken);

        Assert.Equal(1, _unitOfWork.TransactionCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedCurrency_ThrowsAndCreatesNothing()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(Command(currencyCode: "EUR"), TestContext.Current.CancellationToken));

        Assert.Empty(_repository.StoredUsers);
        Assert.Empty(_identityService.Accounts);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_InvalidDisplayName_ThrowsBeforeCreatingAccount()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(Command(displayName: " "), TestContext.Current.CancellationToken));

        Assert.Empty(_identityService.Accounts);
        Assert.Equal(0, _unitOfWork.TransactionCount);
    }

    [Fact]
    public async Task HandleAsync_EmailAlreadyRegistered_ThrowsConflictAndStoresNoProfile()
    {
        await _handler.HandleAsync(Command(), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.HandleAsync(Command(displayName: "Bea"), TestContext.Current.CancellationToken));

        Assert.Single(_repository.StoredUsers);
    }
}