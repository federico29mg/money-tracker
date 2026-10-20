using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Domain.TransferAggregate;

// Money moving between two of the user's own accounts: paying a credit card, saving, buying dollars.
// It is never income or an expense, so it doesn't appear in spending reports or budgets.
public sealed class Transfer
{
    public const int MaxDescriptionLength = 200;

    private Transfer(
        TransferId id,
        UserId userId,
        AccountId fromAccountId,
        AccountId toAccountId,
        Money amount,
        Money receivedAmount,
        DateOnly date,
        string? description)
    {
        Id = id;
        UserId = userId;
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
        ReceivedAmount = receivedAmount;
        Date = date;
        Description = description;
    }

    // Used by EF Core when loading: it can't pass complex types to constructors and sets the Money properties itself.
    private Transfer(
        TransferId id,
        UserId userId,
        AccountId fromAccountId,
        AccountId toAccountId,
        DateOnly date,
        string? description)
    {
        Id = id;
        UserId = userId;
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = null!;
        ReceivedAmount = null!;
        Date = date;
        Description = description;
    }

    public TransferId Id { get; }
    public UserId UserId { get; }
    public AccountId FromAccountId { get; }
    public AccountId ToAccountId { get; }

    // What left the source account, in its currency.
    public Money Amount { get; }

    // What arrived in the destination account, in its currency. Equal to Amount when both accounts share a currency.
    public Money ReceivedAmount { get; }
    public DateOnly Date { get; }
    public string? Description { get; }

    // receivedAmount is only needed between accounts in different currencies (e.g. 500,000 COP in, 120 USD out).
    public static Transfer Record(
        Account from,
        Account to,
        Money amount,
        Money? receivedAmount,
        DateOnly date,
        string? description)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(amount);

        if (from.UserId != to.UserId)
        {
            throw new DomainException("Both accounts must belong to the same user.");
        }

        if (from.Id == to.Id)
        {
            throw new DomainException("A transfer needs two different accounts.");
        }

        if (from.IsArchived || to.IsArchived)
        {
            throw new DomainException($"Account '{(from.IsArchived ? from.Name : to.Name)}' is archived.");
        }

        if (!amount.IsPositive)
        {
            throw new DomainException("Amount must be greater than zero.");
        }

        if (amount.Currency != from.Currency)
        {
            throw new DomainException($"The amount must be in the source account's currency ({from.Currency}).");
        }

        if (date == default)
        {
            throw new DomainException("Date is required.");
        }

        return new Transfer(
            TransferId.New(),
            from.UserId,
            from.Id,
            to.Id,
            amount,
            ReceivedAmountFor(amount, receivedAmount, to),
            date,
            NormalizeDescription(description));
    }

    private static Money ReceivedAmountFor(Money amount, Money? receivedAmount, Account to)
    {
        if (amount.Currency == to.Currency)
        {
            if (receivedAmount is not null && receivedAmount != amount)
            {
                throw new DomainException(
                    "Between accounts in the same currency, the amount received is the amount sent.");
            }

            return amount;
        }

        if (receivedAmount is null)
        {
            throw new DomainException($"Enter how much arrived in {to.Currency}.");
        }

        if (receivedAmount.Currency != to.Currency)
        {
            throw new DomainException(
                $"The amount received must be in the destination account's currency ({to.Currency}).");
        }

        if (!receivedAmount.IsPositive)
        {
            throw new DomainException("The amount received must be greater than zero.");
        }

        return receivedAmount;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();

        if (trimmed.Length > MaxDescriptionLength)
        {
            throw new DomainException($"Description cannot exceed {MaxDescriptionLength} characters.");
        }

        return trimmed;
    }
}