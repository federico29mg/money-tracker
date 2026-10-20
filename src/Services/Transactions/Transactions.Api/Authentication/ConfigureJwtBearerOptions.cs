using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Transactions.Api.Authentication;

// Validates tokens issued by Users: the public keys come from its JWKS, issuer and audience from configuration.
internal sealed class ConfigureJwtBearerOptions(IOptions<JwtValidationOptions> jwtOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            Configure(options);
        }
    }

    public void Configure(JwtBearerOptions options)
    {
        var jwt = jwtOptions.Value;

        // Downloads Users' discovery document and JWKS once, caches the keys and refreshes them when a new key id appears.
        options.MetadataAddress = jwt.MetadataAddress;
        options.RequireHttpsMetadata = jwt.RequireHttpsMetadata;

        // Keep claim names as they are in the token ("sub", "email") instead of mapping them to long .NET URIs.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }
}