using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Transactions.IntegrationTests.Support;

// Hosts Transactions.Api in memory, with the test containers' connection strings and tokens signed by TestTokens.
public sealed class TransactionsApiFactory(string databaseConnectionString, string serviceBusConnectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development, so appsettings.Development.json (localhost addresses) isn't loaded.
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:TransactionsDb", databaseConnectionString);
        builder.UseSetting("ConnectionStrings:ServiceBus", serviceBusConnectionString);

        // Required by the options validation but never contacted: the Users service isn't running in these tests.
        builder.UseSetting("Jwt:MetadataAddress", "https://users.invalid/.well-known/openid-configuration");

        builder.ConfigureTestServices(services =>
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // A fixed configuration replaces the download of Users' discovery document and keys.
                options.Configuration = new OpenIdConnectConfiguration();
                options.TokenValidationParameters.IssuerSigningKey = TestTokens.SigningKey;
            }));
    }
}