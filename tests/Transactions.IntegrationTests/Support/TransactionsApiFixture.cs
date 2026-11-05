using System.Net.Http.Headers;

using Azure.Messaging.ServiceBus;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.MsSql;
using Testcontainers.ServiceBus;

using Transactions.Infrastructure.Persistence;
using Transactions.IntegrationTests.Support;

[assembly: AssemblyFixture(typeof(TransactionsApiFixture))]

namespace Transactions.IntegrationTests.Support;

// Starts SQL Server and the Service Bus emulator once for the whole test run, applies the migrations and hosts the API
// against them. Nothing is reset between tests: every test works with users of its own, which keeps them apart.
public sealed class TransactionsApiFixture : IAsyncLifetime
{
    private const string SqlServerAlias = "sqlserver";
    private const string SqlServerPassword = "Integration-Tests-1!";

    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly ServiceBusContainer _serviceBus;
    private readonly MsSqlContainer _sqlServer;
    private TransactionsApiFactory? _factory;
    private ServiceBusClient? _serviceBusClient;

    public TransactionsApiFixture()
    {
        _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(SqlServerPassword)
            .WithNetwork(_network)
            .WithNetworkAliases(SqlServerAlias)
            .Build();

        // Like in Docker Compose, the emulator keeps its messages in the same SQL Server.
        _serviceBus = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:2.0.1")
            .WithAcceptLicenseAgreement(true)
            .WithConfig(Path.Combine(AppContext.BaseDirectory, "servicebus-emulator", "Config.json"))
            .WithMsSqlContainer(_network, _sqlServer, SqlServerAlias, SqlServerPassword)
            .Build();
    }

    public TransactionsApiFactory Factory => _factory ?? throw NotStarted();

    // For tests that play the Users service's part and publish its events.
    public ServiceBusClient ServiceBus => _serviceBusClient ?? throw NotStarted();

    public async ValueTask InitializeAsync()
    {
        await _network.CreateAsync();
        await _sqlServer.StartAsync();
        await _serviceBus.StartAsync();

        var database = new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
        {
            InitialCatalog = "TransactionsDb"
        };

        _factory = new TransactionsApiFactory(database.ConnectionString, _serviceBus.GetConnectionString());
        _serviceBusClient = new ServiceBusClient(_serviceBus.GetConnectionString());

        await using var scope = _factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TransactionsDbContext>().Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceBusClient is not null)
        {
            await _serviceBusClient.DisposeAsync();
        }

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _serviceBus.DisposeAsync();
        await _sqlServer.DisposeAsync();
        await _network.DisposeAsync();
    }

    public HttpClient CreateClient(Guid userId)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.Create(userId));

        return client;
    }

    private static InvalidOperationException NotStarted() => new("The fixture hasn't been initialized.");
}