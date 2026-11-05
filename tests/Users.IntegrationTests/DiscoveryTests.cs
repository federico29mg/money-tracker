using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Users.IntegrationTests.Support;

namespace Users.IntegrationTests;

// Other services validate Users' tokens with nothing but the published keys; these tests check that's enough.
public class DiscoveryTests(UsersApiFixture fixture)
{
    [Fact]
    public async Task AccessToken_ValidatesAgainstThePublishedKeys()
    {
        using var client = fixture.Factory.CreateClient();
        var email = HttpClientExtensions.NewEmail();
        var userId = await client.RegisterAsync(email);
        var login = await client.LogInAsync(email);

        var keys = new JsonWebKeySet(
            await client.GetStringAsync("/.well-known/jwks.json", TestContext.Current.CancellationToken));

        // The same checks Transactions.Api makes.
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(login.AccessToken,
            new TokenValidationParameters
            {
                ValidIssuer = "moneytracker-users",
                ValidAudience = "moneytracker-api",
                IssuerSigningKeys = keys.GetSigningKeys(),
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
            });

        Assert.True(result.IsValid, result.Exception?.Message);
        Assert.Equal(userId.ToString(), result.Claims[JwtRegisteredClaimNames.Sub]);
    }

    [Fact]
    public async Task OpenIdConfiguration_PointsToTheKeys()
    {
        using var client = fixture.Factory.CreateClient();

        var configuration = await client.GetJsonAsync<Dictionary<string, object>>(
            "/.well-known/openid-configuration");

        Assert.Equal("moneytracker-users", configuration["issuer"].ToString());
        Assert.EndsWith("/.well-known/jwks.json", configuration["jwks_uri"].ToString(), StringComparison.Ordinal);
    }
}