using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Features.Transactions;

public sealed record TransactionResponse(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    DateOnly Date,
    string Name,
    string? Description,
    decimal Amount,
    string Currency,
    decimal? ExchangeRate,
    decimal? SettledAmount,
    string? SettledCurrency,
    bool IsSettled)
{
    public static TransactionResponse From(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return new TransactionResponse(
            transaction.Id.Value,
            transaction.AccountId.Value,
            transaction.CategoryId.Value,
            transaction.Type,
            transaction.Date,
            transaction.Name,
            transaction.Description,
            transaction.Amount.Amount,
            transaction.Amount.Currency.Code,
            transaction.ExchangeRate,
            transaction.SettledAmount?.Amount,
            transaction.SettledAmount?.Currency.Code,
            transaction.IsSettled);
    }
}