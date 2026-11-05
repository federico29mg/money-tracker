using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Users.IntegrationTests.Support;

namespace Users.IntegrationTests;

// These tests play the Transactions service's part: they read what the outbox publishes,
// so they also pin down the message contract between the two services.
public class UserRegisteredTests(UsersApiFixture fixture)
{
    [Fact]
    public async Task Register_PublishesUserRegistered()
    {
        using var client = fixture.Factory.CreateClient();
        var userId = await client.RegisterAsync(HttpClientExtensions.NewEmail(), "USD");

        var message = await ReceiveForUserAsync(userId);

        Assert.NotNull(message);
        Assert.Equal("UserRegistered", message.Subject);
        Assert.Equal("application/json", message.ContentType);
        Assert.True(Guid.TryParse(message.MessageId, out _));
        Assert.Equal(
            $$"""{"userId":"{{userId}}","defaultCurrencyCode":"USD"}""",
            message.Body.ToString());
    }

    // Other tests register users too, so messages are read until this user's one arrives.
    private async Task<ServiceBusReceivedMessage?> ReceiveForUserAsync(Guid userId)
    {
        await using var receiver = fixture.ServiceBus.CreateReceiver("users", "transactions");
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            var message = await receiver.ReceiveMessageAsync(
                TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            if (message is null)
            {
                continue;
            }

            await receiver.CompleteMessageAsync(message, TestContext.Current.CancellationToken);

            using var body = JsonDocument.Parse(message.Body.ToMemory());

            if (body.RootElement.GetProperty("userId").GetGuid() == userId)
            {
                return message;
            }
        }

        return null;
    }
}