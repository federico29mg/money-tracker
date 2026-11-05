using System.Security.Cryptography;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Transactions.IntegrationTests.Support;

// Signs tokens the way the Users service does (RS256, same issuer and audience as appsettings.json),
// with a key that only exists for this test run.
public static class TestTokens
{
    private static readonly JsonWebTokenHandler Handler = new();

    public static RsaSecurityKey SigningKey { get; } = new(RSA.Create(2048));

    public static string Create(Guid userId, SecurityKey? signingKey = null) =>
        Handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "moneytracker-users",
            Audience = "moneytracker-api",
            Expires = DateTime.UtcNow.AddMinutes(10),
            Claims = new Dictionary<string, object> { [JwtRegisteredClaimNames.Sub] = userId.ToString() },
            SigningCredentials = new SigningCredentials(signingKey ?? SigningKey, SecurityAlgorithms.RsaSha256)
        });
}