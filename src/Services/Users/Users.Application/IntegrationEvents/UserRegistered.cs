namespace Users.Application.IntegrationEvents;

public sealed record UserRegistered(Guid UserId, string DefaultCurrencyCode) : IIntegrationEvent;