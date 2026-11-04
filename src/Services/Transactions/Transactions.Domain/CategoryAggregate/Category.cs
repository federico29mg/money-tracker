using Transactions.Domain.Common;

namespace Transactions.Domain.CategoryAggregate;

public sealed class Category
{
    public const string DefaultName = "Uncategorized";
    public const int MaxNameLength = 50;

    private Category(CategoryId id, UserId userId, string name, TransactionType type)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Type = type;
    }

    public CategoryId Id { get; }
    public UserId UserId { get; }
    public string Name { get; private set; }
    public TransactionType Type { get; }
    public bool IsArchived { get; private set; }

    // Used when a transaction is recorded without a category. One per user and type, and it can't be archived.
    public bool IsDefault { get; private set; }

    public static Category Create(UserId userId, string name, TransactionType type)
    {
        if (userId == default)
        {
            throw new DomainException("User id is required.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException($"Transaction type '{type}' is not supported.");
        }

        return new Category(CategoryId.New(), userId, NormalizeName(name), type);
    }

    public static Category CreateDefault(UserId userId, TransactionType type)
    {
        var category = Create(userId, DefaultName, type);
        category.IsDefault = true;

        return category;
    }

    public void Rename(string name) => Name = NormalizeName(name);

    public void Archive()
    {
        if (IsDefault)
        {
            throw new DomainException($"Category '{Name}' is the default {Type} category and can't be archived.");
        }

        IsArchived = true;
    }

    // Turns an existing category into the default, e.g. one the user had already named "Uncategorized".
    public void MakeDefault()
    {
        IsDefault = true;
        IsArchived = false;
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        var trimmed = name.Trim();

        if (trimmed.Length > MaxNameLength)
        {
            throw new DomainException($"Category name cannot exceed {MaxNameLength} characters.");
        }

        return trimmed;
    }
}