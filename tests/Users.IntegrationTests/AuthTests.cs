using System.Net;

using Users.Api.Contracts;
using Users.Application.Features.GetUser;
using Users.IntegrationTests.Support;

namespace Users.IntegrationTests;

public class AuthTests(UsersApiFixture fixture)
{
    [Fact]
    public async Task RegisterThenLogIn_GivesAccessToTheProfile()
    {
        using var client = fixture.Factory.CreateClient();
        var email = HttpClientExtensions.NewEmail();
        var userId = await client.RegisterAsync(email, "usd");

        await client.LogInAsync(email);
        var profile = await client.GetJsonAsync<UserResponse>("/api/users/me");

        Assert.Equal(new UserResponse(userId, "Ana", "USD"), profile);
    }

    [Fact]
    public async Task Register_SameEmailInAnotherCase_Returns409()
    {
        using var client = fixture.Factory.CreateClient();
        var email = HttpClientExtensions.NewEmail();
        await client.RegisterAsync(email);

        using var response = await client.PostJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(email.ToUpperInvariant(), HttpClientExtensions.Password, "Bea", null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task LogIn_WrongPassword_Returns401()
    {
        using var client = fixture.Factory.CreateClient();
        var email = HttpClientExtensions.NewEmail();
        await client.RegisterAsync(email);

        using var response = await client.PostJsonAsync(
            "/api/auth/login", new LoginRequest(email, "not the password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_WithoutToken_Returns401()
    {
        using var client = fixture.Factory.CreateClient();

        using var response = await client.GetAsync("/api/users/me", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}