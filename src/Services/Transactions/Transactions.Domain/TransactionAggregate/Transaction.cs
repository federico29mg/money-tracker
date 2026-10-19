using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Domain.TransactionAggregate;

public sealed class Transaction
{
    public const int MaxDescriptionLength = 200;
    public const int MaxExchangeRateDecimalPlaces = 8;
    public const int MaxNameLength = 50;

    private Transaction(
        TransactionId id,
        UserId userId,
        AccountId accountId,
        CategoryId categoryId,
        TransactionType type,
        Money amount,
        decimal? exchangeRate,
        Money? settledAmount,
        DateOnly date,
        string name,
        string? description)
    {
        Id = id;
        UserId = userId;
        AccountId = accountId;
        CategoryId = categoryId;
        Type = type;
        Amount = amount;
        ExchangeRate = exchangeRate;
        SettledAmount = settledAmount;
        Date = date;
        Name = name;
        Description = description;
    }

    public TransactionId Id { get; }
    public UserId UserId { get; }
    public AccountId AccountId { get; }
    public CategoryId CategoryId { get; }
    public TransactionType Type { get; }

    // What was charged, in the currency it was charged in (e.g. 50 USD on a COP card).
    public Money Amount { get; }

    // Units of the account currency per 1 unit of the amount's currency (e.g. 4103.25 COP per USD).
    // 1 when the currencies match; null until a foreign-currency transaction is settled.
    public decimal? ExchangeRate { get; private set; }

    // What it actually cost in the account's currency: Amount × ExchangeRate, rounded to cents.
    public Money? SettledAmount { get; private set; }
    public DateOnly Date { get; }
    public string Name { get; }
    public string? Description { get; }

    public bool IsSettled => SettledAmount is not null;

    public static Transaction Record(
        Account account,
        Category category,
        TransactionType type,
        Money amount,
        DateOnly date,
        string name,
        string? description)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(amount);

        if (category.UserId != account.UserId)
        {
            throw new DomainException("The category and the account must belong to the same user.");
        }

        if (account.IsArchived)
        {
            throw new DomainException($"Account '{account.Name}' is archived.");
        }

        if (category.IsArchived)
        {
            throw new DomainException($"Category '{category.Name}' is archived.");
        }

        if (category.Type != type)
        {
            throw new DomainException($"Category '{category.Name}' is for {category.Type} transactions, not {type}.");
        }

        if (!amount.IsPositive)
        {
            throw new DomainException("Amount must be greater than zero.");
        }

        if (date == default)
        {
            throw new DomainException("Date is required.");
        }

        // Same currency as the account: the amount is already what it cost.
        var isAccountCurrency = amount.Currency == account.Currency;

        return new Transaction(
            TransactionId.New(),
            account.UserId,
            account.Id,
            category.Id,
            type,
            amount,
            isAccountCurrency ? 1m : null,
            isAccountCurrency ? amount : null,
            date,
            NormalizeName(name),
            NormalizeDescription(description));
    }

    // Applies the rate the bank used when the card was paid; can be called again to correct it.
    public void Settle(Account account, decimal exchangeRate)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.Id != AccountId)
        {
            throw new ArgumentException("The account is not the one this transaction belongs to.", nameof(account));
        }

        if (Amount.Currency == account.Currency)
        {
            throw new DomainException("Transactions in the account currency are settled automatically.");
        }

        if (exchangeRate <= 0)
        {
            throw new DomainException("The exchange rate must be greater than zero.");
        }

        if (decimal.Round(exchangeRate, MaxExchangeRateDecimalPlaces) != exchangeRate)
        {
            throw new DomainException(
                $"The exchange rate cannot have more than {MaxExchangeRateDecimalPlaces} decimal places.");
        }

        // Banks round conversions half away from zero (0.125 -> 0.13); decimal.Round defaults to banker's rounding.
        var converted = decimal.Round(Amount.Amount * exchangeRate, Money.MaxDecimalPlaces,
            MidpointRounding.AwayFromZero);

        ExchangeRate = exchangeRate;
        SettledAmount = Money.Of(converted, account.Currency);
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();

        return trimmed.Length > MaxDescriptionLength
            ? throw new DomainException($"Description cannot exceed {MaxDescriptionLength} characters.")
            : trimmed;
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Transaction name is required.");
        }

        var trimmed = name.Trim();

        if (trimmed.Length > MaxNameLength)
        {
            throw new DomainException($"Transaction name cannot exceed {MaxNameLength} characters.");
        }

        return trimmed;
    }
}