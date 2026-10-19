namespace Transactions.Domain.Common;

// Users are created by the Users service; here the id only arrives from the access token.
public readonly record struct UserId
{
    private UserId(Guid value) => Value = value;
    public Guid Value { get; }

    public static UserId From(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException("User id cannot be empty.")
            : new UserId(value);

    public override string ToString() => Value.ToString();
}