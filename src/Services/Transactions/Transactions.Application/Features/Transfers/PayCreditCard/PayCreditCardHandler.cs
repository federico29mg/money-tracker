using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Application.Features.Accounts;
using Transactions.Application.Features.Transactions.SettleTransactions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.Application.Features.Transfers.PayCreditCard;

public sealed class PayCreditCardHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    ITransferRepository transferRepository,
    IUnitOfWork unitOfWork)
{
    private const string DefaultDescription = "Card payment";

    private readonly TransactionSettler _settler = new(transactionRepository);

    public async Task<PayCreditCardResponse> HandleAsync(
        PayCreditCardCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var card = await accountRepository.GetRequiredAsync(command.CardAccountId, command.UserId, cancellationToken);
        var from = await accountRepository.GetRequiredAsync(command.FromAccountId, command.UserId, cancellationToken);

        if (card.Type != AccountType.CreditCard)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(command.CardAccountId)] = [$"Account '{card.Name}' is not a credit card."]
            });
        }

        var transfer = Transfer.Record(
            from,
            card,
            Money.Of(command.Amount, from.Currency),
            command.ReceivedAmount is { } received ? Money.Of(received, card.Currency) : null,
            command.Date,
            string.IsNullOrWhiteSpace(command.Description) ? DefaultDescription : command.Description);

        // Settled before anything is added, so a rejected settlement leaves no half-recorded payment behind.
        var settlement = command.Settlement is { } covered
            ? await _settler.SettleAsync(
                card, covered.TransactionIds, covered.ExchangeRate, covered.TotalPaid, cancellationToken)
            : null;

        transferRepository.Add(transfer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PayCreditCardResponse(transfer.Id.Value, settlement);
    }
}