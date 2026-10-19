using Transactions.Domain.Common;

namespace Transactions.Domain.CategoryAggregate;

public readonly record struct CategoryId
{
    private CategoryId(Guid value) => Value = value;
    public Guid Value { get; }

    public static CategoryId New() => new(Guid.CreateVersion7());

    public static CategoryId From(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException("Category id cannot be empty.")
            : new CategoryId(value);

    public override string ToString() => Value.ToString();
}