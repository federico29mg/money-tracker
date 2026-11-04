using Users.Application.IntegrationEvents;

namespace Users.Application.Abstractions;

// Integration events are saved with the rest of the unit of work and sent to the broker afterwards,
// so an event goes out only if the change that raised it was committed.
public interface IOutbox
{
    void Add(IIntegrationEvent integrationEvent);
}