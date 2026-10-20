using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Transactions.Domain.Common;

namespace Transactions.Infrastructure.Persistence.Configurations;

internal static class MoneyMapping
{
    // Maps a Money complex property to two columns: an amount and a three-letter currency code.
    public static void MapMoney(this ComplexPropertyBuilder<Money> money, string amountColumn, string currencyColumn)
    {
        money.Property(m => m.Amount)
            .HasColumnName(amountColumn)
            .HasPrecision(ColumnTypes.AmountPrecision, ColumnTypes.AmountScale);

        money.Property(m => m.Currency)
            .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
            .HasColumnName(currencyColumn)
            .HasMaxLength(ColumnTypes.CurrencyCodeLength)
            .IsFixedLength()
            .IsUnicode(false);
    }
}