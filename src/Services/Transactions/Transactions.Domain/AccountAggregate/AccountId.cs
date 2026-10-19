using Transactions.Domain.Common;

namespace Transactions.Domain.AccountAggregate;

public readonly record struct AccountId
{
    private AccountId(Guid value) => Value = value;
    public Guid Value { get; }

    public static AccountId New() => new(Guid.CreateVersion7());

    public static AccountId From(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException("Account id cannot be empty.")
            : new AccountId(value);

    public override string ToString() => Value.ToString();
}