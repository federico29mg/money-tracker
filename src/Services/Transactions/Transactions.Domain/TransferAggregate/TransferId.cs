using Transactions.Domain.Common;

namespace Transactions.Domain.TransferAggregate;

public readonly record struct TransferId
{
    private TransferId(Guid value) => Value = value;
    public Guid Value { get; }

    public static TransferId New() => new(Guid.CreateVersion7());

    public static TransferId From(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException("Transfer id cannot be empty.")
            : new TransferId(value);

    public override string ToString() => Value.ToString();
}