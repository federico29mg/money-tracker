namespace Users.Application.IntegrationEvents;

// A fact other services may react to. Its name and fields are a contract: other services keep their own copy,
// so fields are only ever added, never renamed or removed.
public interface IIntegrationEvent;