namespace Transactions.Application.Features.Transfers.GetTransfers;

public sealed record GetTransfersQuery(
    Guid UserId,
    Guid? AccountId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = GetTransfersQuery.DefaultPageSize)
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;
}