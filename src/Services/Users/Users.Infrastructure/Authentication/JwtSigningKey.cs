using System.Security.Cryptography;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Users.Infrastructure.Authentication;

// Loads the RSA key once. The private key signs tokens; the public key validates them and is published as JWKS.
public sealed class JwtSigningKey : IDisposable
{
    private readonly RSA _rsa;

    public JwtSigningKey(IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _rsa = RSA.Create();
        _rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(options.Value.SigningKey), out _);

        var publicParameters = _rsa.ExportParameters(false);
        PublicKey = new RsaSecurityKey(publicParameters);
        PublicKey.KeyId = Base64UrlEncoder.Encode(PublicKey.ComputeJwkThumbprint());

        PrivateKey = new RsaSecurityKey(_rsa) { KeyId = PublicKey.KeyId };
        Modulus = Base64UrlEncoder.Encode(publicParameters.Modulus);
        Exponent = Base64UrlEncoder.Encode(publicParameters.Exponent);
    }

    public RsaSecurityKey PrivateKey { get; }

    public RsaSecurityKey PublicKey { get; }

    public string KeyId => PublicKey.KeyId;

    // Base64url-encoded public key components, as required by the JWKS format.
    public string Modulus { get; }

    public string Exponent { get; }

    public void Dispose() => _rsa.Dispose();

    internal static bool IsValidPrivateKey(string base64Key)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(base64Key), out _);
            return rsa.KeySize >= 2048;
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            return false;
        }
    }
}