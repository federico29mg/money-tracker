using Users.Application.Common;
using Users.Application.Features.GetUser;
using Users.Domain.Common;
using Users.Domain.UserAggregate;
using Users.UnitTests.Fakes;

namespace Users.UnitTests;

public class GetUserHandlerTests
{
    private readonly GetUserHandler _handler;
    private readonly InMemoryUserRepository _repository = new();

    public GetUserHandlerTests() => _handler = new GetUserHandler(_repository);

    [Fact]
    public async Task HandleAsync_ExistingUser_ReturnsMappedResponse()
    {
        var user = User.Register(UserId.New(), "Ana", UserSettings.Create(Currency.Usd));
        _repository.Add(user);

        var response = await _handler.HandleAsync(
            new GetUserQuery(user.Id.Value), TestContext.Current.CancellationToken);

        Assert.Equal(new UserResponse(user.Id.Value, "Ana", "USD"), response);
    }

    [Fact]
    public async Task HandleAsync_UnknownUser_ThrowsNotFoundException()
    {
        var query = new GetUserQuery(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(query, TestContext.Current.CancellationToken));
    }
}