namespace Transactions.Application.Features.Accounts.RenameAccount;

public sealed record RenameAccountCommand(Guid UserId, Guid AccountId, string Name);