namespace Transactions.Application.Features.Transactions.GetTransaction;

public sealed record GetTransactionQuery(Guid UserId, Guid TransactionId);