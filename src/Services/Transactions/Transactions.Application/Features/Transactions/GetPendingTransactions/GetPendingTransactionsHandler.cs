using Transactions.Application.Abstractions;
using Transactions.Application.Features.Accounts;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Features.Transactions.GetPendingTransactions;

public sealed class GetPendingTransactionsHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
{
    public async Task<PendingTransactionsResponse> HandleAsync(
        GetPendingTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var account = await accountRepository.GetRequiredAsync(query.AccountId, query.UserId, cancellationToken);
        var pending = await transactionRepository.ListPendingAsync(account.Id, account.UserId, cancellationToken);

        var groups = pending
            .GroupBy(transaction => transaction.Amount.Currency)
            .OrderBy(group => group.Key.Code, StringComparer.Ordinal)
            .Select(group => new PendingCurrencyGroup(
                group.Key.Code,
                group.Sum(SignedAmount),
                [.. group.Select(TransactionResponse.From)]))
            .ToList();

        return new PendingTransactionsResponse(account.Id.Value, account.Currency.Code, groups);
    }

    private static decimal SignedAmount(Transaction transaction) =>
        transaction.Type == TransactionType.Income ? transaction.Amount.Amount : -transaction.Amount.Amount;
}