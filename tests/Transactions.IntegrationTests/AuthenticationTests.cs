using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;

using Transactions.Api.Contracts;
using Transactions.Domain.AccountAggregate;
using Transactions.IntegrationTests.Support;

namespace Transactions.IntegrationTests;

public class AuthenticationTests(TransactionsApiFixture fixture)
{
    [Fact]
    public async Task Request_WithoutToken_Returns401()
    {
        using var client = fixture.Factory.CreateClient();

        using var response = await client.GetAsync("/api/accounts", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithTokenSignedByAnotherKey_Returns401()
    {
        using var otherKey = RSA.Create(2048);
        using var client = fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            TestTokens.Create(Guid.NewGuid(), new RsaSecurityKey(otherKey)));

        using var response = await client.GetAsync("/api/accounts", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Account_OfAnotherUser_Returns404()
    {
        using var owner = fixture.CreateClient(Guid.NewGuid());
        using var stranger = fixture.CreateClient(Guid.NewGuid());
        var accountId = await owner.CreateAsync(
            "/api/accounts", new OpenAccountRequest("Checking", AccountType.Checking, "COP", 1_000m));

        using var response = await stranger.GetAsync(
            $"/api/accounts/{accountId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}