using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Transactions.Application.Features.Accounts;
using Transactions.Application.Features.Categories;
using Transactions.Domain.AccountAggregate;
using Transactions.IntegrationTests.Support;

namespace Transactions.IntegrationTests;

// These tests play the Users service's part: they publish UserRegistered the way its outbox does,
// so they also pin down the message contract between the two services.
public class UserProvisioningTests(TransactionsApiFixture fixture) : IAsyncDisposable
{
    private readonly ServiceBusSender _sender = fixture.ServiceBus.CreateSender("users");

    public ValueTask DisposeAsync() => _sender.DisposeAsync();

    [Fact]
    public async Task UserRegistered_ProvisionsCashAccountAndDefaultCategories()
    {
        var userId = Guid.NewGuid();
        using var client = fixture.CreateClient(userId);

        await PublishAsync(UserRegistered(userId, "USD"));

        var accounts = await Eventually.GetAsync(
            () => client.GetJsonAsync<List<AccountResponse>>("/api/accounts"), accounts => accounts.Count > 0);

        var cash = Assert.Single(accounts);
        Assert.Equal(("Cash", AccountType.Cash, "USD"), (cash.Name, cash.Type, cash.Currency));

        var categories = await client.GetJsonAsync<List<CategoryResponse>>("/api/categories");
        Assert.Equal(2, categories.Count(category => category is { IsDefault: true, Name: "Uncategorized" }));
    }

    [Fact]
    public async Task UserRegistered_DeliveredTwice_ProvisionsOnce()
    {
        var userId = Guid.NewGuid();
        var marker = Guid.NewGuid();
        using var client = fixture.CreateClient(userId);
        using var markerClient = fixture.CreateClient(marker);

        await PublishAsync(UserRegistered(userId, "COP"));
        await PublishAsync(UserRegistered(userId, "COP"));

        // Messages are handled one at a time, in order: once the marker user exists, both copies have been handled.
        await PublishAsync(UserRegistered(marker, "COP"));
        await Eventually.GetAsync(
            () => markerClient.GetJsonAsync<List<AccountResponse>>("/api/accounts"), accounts => accounts.Count > 0);

        Assert.Single(await client.GetJsonAsync<List<AccountResponse>>("/api/accounts"));
        Assert.Equal(2, (await client.GetJsonAsync<List<CategoryResponse>>("/api/categories")).Count);
    }

    [Fact]
    public async Task UserRegistered_WithUnsupportedCurrency_IsDeadLettered()
    {
        var message = UserRegistered(Guid.NewGuid(), "XYZ");

        await PublishAsync(message);

        await using var deadLetters = fixture.ServiceBus.CreateReceiver(
            "users", "transactions", new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });

        var deadLettered = await Eventually.GetAsync(
            () => deadLetters.ReceiveMessageAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            received => received?.MessageId == message.MessageId);

        Assert.Equal("InvalidEvent", deadLettered?.DeadLetterReason);
    }

    private static ServiceBusMessage UserRegistered(Guid userId, string currencyCode) =>
        new(JsonSerializer.Serialize(new { userId, defaultCurrencyCode = currencyCode }))
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = "UserRegistered",
            ContentType = "application/json"
        };

    private Task PublishAsync(ServiceBusMessage message) =>
        _sender.SendMessageAsync(message, TestContext.Current.CancellationToken);
}