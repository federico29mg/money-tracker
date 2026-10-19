namespace Transactions.Application.Features.Transactions.GetTransactions;

public sealed record GetTransactionsQuery(
    Guid UserId,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = GetTransactionsQuery.DefaultPageSize)
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;
}