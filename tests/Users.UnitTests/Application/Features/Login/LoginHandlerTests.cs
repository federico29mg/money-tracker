using Users.Application.Common;
using Users.Application.Features.Login;
using Users.Domain.UserAggregate;
using Users.UnitTests.Fakes;

namespace Users.UnitTests.Application.Features.Login;

public class LoginHandlerTests
{
    private const string Email = "ana@example.com";
    private const string Password = "correct horse battery";

    private readonly LoginHandler _handler;
    private readonly FakeIdentityService _identityService = new();
    private readonly UserId _userId = UserId.New();

    public LoginHandlerTests() => _handler = new LoginHandler(_identityService, new FakeAccessTokenIssuer());

    private Task RegisterAccountAsync() =>
        _identityService.CreateUserAsync(_userId, Email, Password, TestContext.Current.CancellationToken);

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsTokenForUser()
    {
        await RegisterAccountAsync();

        var response = await _handler.HandleAsync(new LoginCommand(Email, Password), TestContext.Current.CancellationToken);

        Assert.Equal($"token-for-{_userId}", response.AccessToken);
        Assert.Equal(FakeAccessTokenIssuer.ExpiresAtUtc, response.ExpiresAtUtc);
        Assert.Equal("Bearer", response.TokenType);
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        await RegisterAccountAsync();

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.HandleAsync(new LoginCommand(Email, "wrong password!!"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_UnknownEmail_ThrowsInvalidCredentials()
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.HandleAsync(new LoginCommand("nobody@example.com", Password), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void LoginCommand_ToString_DoesNotContainPassword()
    {
        var command = new LoginCommand(Email, Password);

        Assert.DoesNotContain(Password, command.ToString(), StringComparison.Ordinal);
    }
}