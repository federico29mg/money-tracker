using System.Security.Cryptography;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Users.IntegrationTests.Support;

// Hosts Users.Api in memory, with the test containers' connection strings and a signing key made for this run.
public sealed class UsersApiFactory(string databaseConnectionString, string serviceBusConnectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development, so appsettings.Development.json (localhost addresses) isn't loaded.
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:UsersDb", databaseConnectionString);
        builder.UseSetting("ConnectionStrings:ServiceBus", serviceBusConnectionString);
        builder.UseSetting("Jwt:SigningKey", CreateSigningKey());
    }

    // The same format scripts/new-jwt-key.ps1 produces: a base64 PKCS#8 RSA private key.
    private static string CreateSigningKey()
    {
        using var rsa = RSA.Create(2048);

        return Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
    }
}