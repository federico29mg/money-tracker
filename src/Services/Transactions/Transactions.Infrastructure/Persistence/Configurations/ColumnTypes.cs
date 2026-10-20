namespace Transactions.Infrastructure.Persistence.Configurations;

internal static class ColumnTypes
{
    public const int CurrencyCodeLength = 3;
    public const int EnumLength = 20;

    // Up to 9,999,999,999,999,999.99: two decimals, like Money.
    public const int AmountPrecision = 18;
    public const int AmountScale = 2;

    // Up to 9,999,999,999.99999999: eight decimals, like Transaction.MaxExchangeRateDecimalPlaces.
    public const int ExchangeRatePrecision = 18;
    public const int ExchangeRateScale = 8;
}