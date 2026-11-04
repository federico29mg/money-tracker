namespace Transactions.Application.Features.Provisioning.ProvisionUser;

// CurrencyCode is the user's default currency, used for their Cash account.
public sealed record ProvisionUserCommand(Guid UserId, string CurrencyCode);