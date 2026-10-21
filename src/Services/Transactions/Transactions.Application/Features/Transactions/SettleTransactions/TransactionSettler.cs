using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Features.Transactions.SettleTransactions;

// The settlement rules shared by "Settle" and "Pay card". It changes the transactions but doesn't save,
// so each caller decides what else goes into the same save.
internal sealed class TransactionSettler(ITransactionRepository transactionRepository)
{
    public async Task<SettlementResponse> SettleAsync(
        Account account,
        IReadOnlyList<Guid>? transactionIds,
        decimal? exchangeRate,
        decimal? totalPaid,
        CancellationToken cancellationToken)
    {
        ValidateInput(transactionIds, exchangeRate, totalPaid);

        var ids = transactionIds!.Distinct().Select(TransactionId.From).ToList();
        var transactions = await transactionRepository.ListByIdsAsync(ids, account.UserId, cancellationToken);

        if (transactions.Count != ids.Count)
        {
            throw new NotFoundException("One or more transactions were not found.");
        }

        if (transactions.Any(transaction => transaction.AccountId != account.Id))
        {
            throw Invalid("All transactions must belong to the account being settled.");
        }

        var currencies = transactions.Select(transaction => transaction.Amount.Currency).Distinct().ToList();

        if (currencies.Count > 1)
        {
            throw Invalid("All transactions must be in the same currency; settle each currency separately.");
        }

        var currency = currencies[0];

        if (currency == account.Currency)
        {
            throw Invalid($"Transactions in {account.Currency} are already settled in this account.");
        }

        var rate = exchangeRate ?? RateFromTotal(totalPaid!.Value, transactions);

        foreach (var transaction in transactions)
        {
            transaction.Settle(account, rate);
        }

        return new SettlementResponse(
            transactions.Count,
            currency.Code,
            rate,
            account.Currency.Code,
            transactions.Sum(transaction => transaction.SettledAmount?.Amount ?? 0m));
    }

    private static void ValidateInput(IReadOnlyList<Guid>? transactionIds, decimal? exchangeRate, decimal? totalPaid)
    {
        var errors = new Dictionary<string, string[]>();

        if (transactionIds is null or { Count: 0 })
        {
            errors[nameof(SettleTransactionsCommand.TransactionIds)] = ["At least one transaction is required."];
        }

        if (exchangeRate.HasValue == totalPaid.HasValue)
        {
            errors[nameof(SettleTransactionsCommand.ExchangeRate)] =
                ["Provide either an exchange rate or the total paid, not both."];
        }

        if (totalPaid <= 0)
        {
            errors[nameof(SettleTransactionsCommand.TotalPaid)] = ["The total paid must be greater than zero."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    // The rate that turns the foreign total into what was paid, rounded to the precision Settle accepts.
    private static decimal RateFromTotal(decimal totalPaid, IReadOnlyList<Transaction> transactions) =>
        decimal.Round(
            totalPaid / transactions.Sum(transaction => transaction.Amount.Amount),
            Transaction.MaxExchangeRateDecimalPlaces,
            MidpointRounding.AwayFromZero);

    private static ValidationException Invalid(string message) =>
        new(new Dictionary<string, string[]> { [nameof(SettleTransactionsCommand.TransactionIds)] = [message] });
}