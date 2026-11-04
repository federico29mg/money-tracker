using Users.Application.Abstractions;
using Users.Application.IntegrationEvents;

namespace Users.UnitTests.Fakes;

public sealed class FakeOutbox : IOutbox
{
    private readonly List<IIntegrationEvent> _events = [];

    public IReadOnlyList<IIntegrationEvent> Events => _events;

    public void Add(IIntegrationEvent integrationEvent) => _events.Add(integrationEvent);
}