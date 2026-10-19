namespace Transactions.Application.Features.Accounts.GetAccounts;

public sealed record GetAccountsQuery(Guid UserId, bool IncludeArchived = false);