using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Transfers.GetTransfers;

public sealed class GetTransfersHandler(ITransferRepository transferRepository)
{
    public async Task<PagedResponse<TransferResponse>> HandleAsync(
        GetTransfersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        Validate(query);

        var userId = UserId.From(query.UserId);
        var filter = new TransferFilter(
            query.AccountId is { } accountId ? AccountId.From(accountId) : null,
            query.From,
            query.To);

        var transfers = await transferRepository.ListAsync(
            userId, filter, (query.Page - 1) * query.PageSize, query.PageSize, cancellationToken);
        var totalCount = await transferRepository.CountAsync(userId, filter, cancellationToken);

        return new PagedResponse<TransferResponse>(
            [.. transfers.Select(TransferResponse.From)], query.Page, query.PageSize, totalCount);
    }

    private static void Validate(GetTransfersQuery query)
    {
        var errors = new Dictionary<string, string[]>();

        if (query.Page < 1)
        {
            errors[nameof(query.Page)] = ["Page must be 1 or greater."];
        }

        if (query.PageSize is < 1 or > GetTransfersQuery.MaxPageSize)
        {
            errors[nameof(query.PageSize)] = [$"Page size must be between 1 and {GetTransfersQuery.MaxPageSize}."];
        }

        if (query.From > query.To)
        {
            errors[nameof(query.From)] = ["From cannot be later than To."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}