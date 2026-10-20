using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id)
            .HasConversion(id => id.Value, value => AccountId.From(value))
            .ValueGeneratedNever();

        builder.Property(account => account.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value));

        builder.Property(account => account.Name)
            .HasMaxLength(Account.MaxNameLength)
            .IsRequired();

        builder.Property(account => account.Type)
            .HasConversion<string>()
            .HasMaxLength(ColumnTypes.EnumLength);

        builder.ComplexProperty(account => account.OpeningBalance, money =>
            money.MapMoney("OpeningBalance", "Currency"));

        builder.HasIndex(account => account.UserId);
    }
}