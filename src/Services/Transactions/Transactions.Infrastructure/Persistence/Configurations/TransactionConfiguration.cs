using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Infrastructure.Persistence.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .HasConversion(id => id.Value, value => TransactionId.From(value))
            .ValueGeneratedNever();

        builder.Property(transaction => transaction.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value));

        builder.Property(transaction => transaction.AccountId)
            .HasConversion(id => id.Value, value => AccountId.From(value));

        builder.Property(transaction => transaction.CategoryId)
            .HasConversion(id => id.Value, value => CategoryId.From(value));

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(ColumnTypes.EnumLength);

        builder.Property(transaction => transaction.Name)
            .HasMaxLength(Transaction.MaxNameLength)
            .IsRequired();

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(Transaction.MaxDescriptionLength);

        builder.ComplexProperty(transaction => transaction.Amount, money =>
            money.MapMoney("Amount", "Currency"));

        builder.Property(transaction => transaction.ExchangeRate)
            .HasPrecision(ColumnTypes.ExchangeRatePrecision, ColumnTypes.ExchangeRateScale);

        builder.ComplexProperty(transaction => transaction.SettledAmount, money =>
            money.MapMoney("SettledAmount", "SettledCurrency"));

        // Aggregates reference each other by id; the foreign keys only protect integrity in the database.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(transaction => transaction.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(transaction => transaction.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lists and date ranges per user, and balances and pending charges per account.
        builder.HasIndex(transaction => new { transaction.UserId, transaction.Date });
        builder.HasIndex(transaction => new { transaction.AccountId, transaction.Date });
    }
}