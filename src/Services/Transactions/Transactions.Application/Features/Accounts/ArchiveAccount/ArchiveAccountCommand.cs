namespace Transactions.Application.Features.Accounts.ArchiveAccount;

public sealed record ArchiveAccountCommand(Guid UserId, Guid AccountId);