using Transactions.Domain.Common;

namespace Transactions.Domain.AccountAggregate;

public sealed class Account
{
    public const int MaxNameLength = 100;

    private Account(AccountId id, UserId userId, string name, AccountType type, Money openingBalance)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Type = type;
        OpeningBalance = openingBalance;
    }

    public AccountId Id { get; }
    public UserId UserId { get; }
    public string Name { get; private set; }
    public AccountType Type { get; }

    // Can be negative, e.g. a credit card that already has debt when it's added.
    public Money OpeningBalance { get; }
    public Currency Currency => OpeningBalance.Currency;
    public bool IsArchived { get; private set; }

    public static Account Open(UserId userId, string name, AccountType type, Money openingBalance)
    {
        if (userId == default)
        {
            throw new DomainException("User id is required.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException($"Account type '{type}' is not supported.");
        }

        ArgumentNullException.ThrowIfNull(openingBalance);

        return new Account(AccountId.New(), userId, NormalizeName(name), type, openingBalance);
    }

    public void Rename(string name) => Name = NormalizeName(name);

    public void Archive() => IsArchived = true;

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Account name is required.");
        }

        var trimmed = name.Trim();

        if (trimmed.Length > MaxNameLength)
        {
            throw new DomainException($"Account name cannot exceed {MaxNameLength} characters.");
        }

        return trimmed;
    }
}