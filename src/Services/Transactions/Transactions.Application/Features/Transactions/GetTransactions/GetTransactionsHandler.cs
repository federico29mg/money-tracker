using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Transactions.GetTransactions;

public sealed class GetTransactionsHandler(ITransactionRepository transactionRepository)
{
    public async Task<PagedResponse<TransactionResponse>> HandleAsync(
        GetTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        Validate(query);

        var userId = UserId.From(query.UserId);
        var filter = new TransactionFilter(
            query.AccountId is { } accountId ? AccountId.From(accountId) : null,
            query.CategoryId is { } categoryId ? CategoryId.From(categoryId) : null,
            query.From,
            query.To);

        var transactions = await transactionRepository.ListAsync(
            userId, filter, (query.Page - 1) * query.PageSize, query.PageSize, cancellationToken);
        var totalCount = await transactionRepository.CountAsync(userId, filter, cancellationToken);

        return new PagedResponse<TransactionResponse>(
           [.. transactions.Select(TransactionResponse.From)], query.Page, query.PageSize, totalCount);
    }

    private static void Validate(GetTransactionsQuery query)
    {
        var errors = new Dictionary<string, string[]>();

        if (query.Page < 1)
        {
            errors[nameof(query.Page)] = ["Page must be 1 or greater."];
        }

        if (query.PageSize is < 1 or > GetTransactionsQuery.MaxPageSize)
        {
            errors[nameof(query.PageSize)] =
                [$"Page size must be between 1 and {GetTransactionsQuery.MaxPageSize}."];
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