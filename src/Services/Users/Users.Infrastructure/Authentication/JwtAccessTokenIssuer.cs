using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Users.Application.Abstractions;
using Users.Domain.UserAggregate;

namespace Users.Infrastructure.Authentication;

internal sealed class JwtAccessTokenIssuer(
    IOptions<JwtOptions> options,
    JwtSigningKey signingKey,
    TimeProvider timeProvider) : IAccessTokenIssuer
{
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public AccessToken Issue(UserId userId, string email)
    {
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(jwt.AccessTokenLifetimeMinutes);

        var token = _tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = userId.Value.ToString(),
                [JwtRegisteredClaimNames.Email] = email,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            },
            SigningCredentials = new SigningCredentials(signingKey.PrivateKey, SecurityAlgorithms.RsaSha256)
        });

        return new AccessToken(token, expiresAt);
    }
}