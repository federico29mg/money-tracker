using Transactions.Domain.Common;

namespace Transactions.Domain.TransactionAggregate;

public readonly record struct TransactionId
{
    private TransactionId(Guid value) => Value = value;
    public Guid Value { get; }

    public static TransactionId New() => new(Guid.CreateVersion7());

    public static TransactionId From(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException("Transaction id cannot be empty.")
            : new TransactionId(value);

    public override string ToString() => Value.ToString();
}