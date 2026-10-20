namespace Transactions.Application.Features.Transactions.GetPendingTransactions;

public sealed record GetPendingTransactionsQuery(Guid UserId, Guid AccountId);