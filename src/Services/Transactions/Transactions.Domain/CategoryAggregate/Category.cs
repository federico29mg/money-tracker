using Transactions.Domain.Common;

namespace Transactions.Domain.CategoryAggregate;

public sealed class Category
{
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

    public void Rename(string name) => Name = NormalizeName(name);

    public void Archive() => IsArchived = true;

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