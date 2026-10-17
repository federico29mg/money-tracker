using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Users.Infrastructure.Authentication;

namespace Users.Api.Controllers;

// Publishes the public signing key in the standard OpenID Connect shape, so other services can validate tokens.
[ApiController]
[Route(".well-known")]
[AllowAnonymous]
public sealed class DiscoveryController(IOptions<JwtOptions> jwtOptions, JwtSigningKey signingKey) : ControllerBase
{
    private static readonly string[] SupportedAlgorithms = ["RS256"];
    
    [HttpGet("openid-configuration")]
    public IActionResult GetConfiguration() => Ok(new Dictionary<string, object>
    {
        ["issuer"] = jwtOptions.Value.Issuer,
        ["jwks_uri"] = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/.well-known/jwks.json",
        ["id_token_signing_alg_values_supported"] = SupportedAlgorithms
    });

    [HttpGet("jwks.json")]
    public IActionResult GetJsonWebKeySet() => Ok(new
    {
        keys = new[]
        {
            new Dictionary<string, string>
            {
                ["kty"] = "RSA",
                ["use"] = "sig",
                ["alg"] = "RS256",
                ["kid"] = signingKey.KeyId,
                ["n"] = signingKey.Modulus,
                ["e"] = signingKey.Exponent
            }
        }
    });
}