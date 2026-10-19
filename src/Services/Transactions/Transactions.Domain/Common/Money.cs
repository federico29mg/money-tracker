namespace Transactions.Domain.Common;

public sealed record Money
{
    public const int MaxDecimalPlaces  = 2;

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public decimal Amount { get; }
    public Currency Currency { get; }
    
    public bool IsPositive => Amount > 0;

    public static Money Of(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        if (decimal.Round(amount, MaxDecimalPlaces) != amount)
        {
            throw new DomainException($"Amount cannot have more than {MaxDecimalPlaces} decimal places.");
        }
        
        return new Money(amount, currency);
    }
    
    public static Money Zero(Currency currency) => Of(decimal.Zero, currency);
    
    public override string ToString() => $"{Amount} {Currency}";
}