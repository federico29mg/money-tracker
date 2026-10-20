using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Application.Features.Accounts;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Features.Transactions.SettleTransactions;

public sealed class SettleTransactionsHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<SettlementResponse> HandleAsync(
        SettleTransactionsCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidateInput(command);

        var account = await accountRepository.GetRequiredAsync(command.AccountId, command.UserId, cancellationToken);

        var ids = command.TransactionIds.Distinct().Select(TransactionId.From).ToList();
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

        var exchangeRate = command.ExchangeRate ?? RateFromTotal(command.TotalPaid!.Value, transactions);

        foreach (var transaction in transactions)
        {
            transaction.Settle(account, exchangeRate);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SettlementResponse(
            transactions.Count,
            currency.Code,
            exchangeRate,
            account.Currency.Code,
            transactions.Sum(transaction => transaction.SettledAmount?.Amount ?? 0m));
    }

    private static void ValidateInput(SettleTransactionsCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (command.TransactionIds is null or { Count: 0 })
        {
            errors[nameof(command.TransactionIds)] = ["At least one transaction is required."];
        }

        if (command.ExchangeRate.HasValue == command.TotalPaid.HasValue)
        {
            errors[nameof(command.ExchangeRate)] = ["Provide either an exchange rate or the total paid, not both."];
        }

        if (command.TotalPaid <= 0)
        {
            errors[nameof(command.TotalPaid)] = ["The total paid must be greater than zero."];
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