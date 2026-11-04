using System.Text.Json;

using Users.Application.Abstractions;
using Users.Application.IntegrationEvents;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Messaging;

internal sealed class Outbox(UsersDbContext dbContext, TimeProvider timeProvider) : IOutbox
{
    public void Add(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var type = integrationEvent.GetType();

        // Only tracked here: the handler's SaveChanges writes it together with the user.
        dbContext.Set<OutboxMessage>().Add(new OutboxMessage(
            Guid.CreateVersion7(),
            type.Name,
            JsonSerializer.Serialize(integrationEvent, type, JsonSerializerOptions.Web),
            timeProvider.GetUtcNow()));
    }
}