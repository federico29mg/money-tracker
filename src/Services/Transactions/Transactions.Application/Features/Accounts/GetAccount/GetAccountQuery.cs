namespace Transactions.Application.Features.Accounts.GetAccount;

public sealed record GetAccountQuery(Guid UserId, Guid AccountId);