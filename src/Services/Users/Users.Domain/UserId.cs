using Users.Domain.Common;

namespace Users.Domain;

public readonly record struct UserId
{
    public Guid Value { get; }
    
    private UserId(Guid value) => Value = value;

    public static UserId New() => new(Guid.CreateVersion7());
    
    public static UserId From(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException("User id cannot be empty.") 
            : new UserId(value);
    
    public override string ToString() => Value.ToString();
}