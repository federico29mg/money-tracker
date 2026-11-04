namespace Transactions.Infrastructure.Messaging;

// Transactions' own copy of the event the Users service publishes; the services share no code, only this shape.
internal sealed record UserRegistered(Guid UserId, string DefaultCurrencyCode);