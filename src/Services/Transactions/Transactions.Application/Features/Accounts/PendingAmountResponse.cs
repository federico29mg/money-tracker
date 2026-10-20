namespace Transactions.Application.Features.Accounts;

public sealed record PendingAmountResponse(string Currency, decimal Amount);