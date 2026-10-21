using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;
using Transactions.Domain.TransferAggregate;

namespace Transactions.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers");

        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Id)
            .HasConversion(id => id.Value, value => TransferId.From(value))
            .ValueGeneratedNever();

        builder.Property(transfer => transfer.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value));

        builder.Property(transfer => transfer.FromAccountId)
            .HasConversion(id => id.Value, value => AccountId.From(value));

        builder.Property(transfer => transfer.ToAccountId)
            .HasConversion(id => id.Value, value => AccountId.From(value));

        builder.Property(transfer => transfer.Description)
            .HasMaxLength(Transfer.MaxDescriptionLength);

        builder.ComplexProperty(transfer => transfer.Amount, money =>
            money.MapMoney("Amount", "Currency"));

        builder.ComplexProperty(transfer => transfer.ReceivedAmount, money =>
            money.MapMoney("ReceivedAmount", "ReceivedCurrency"));

        // Two foreign keys to the same table; Restrict keeps SQL Server from rejecting them as cascade cycles.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(transfer => transfer.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(transfer => transfer.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lists per user, and balances per source and destination account.
        builder.HasIndex(transfer => new { transfer.UserId, transfer.Date });
        builder.HasIndex(transfer => transfer.FromAccountId);
        builder.HasIndex(transfer => transfer.ToAccountId);
    }
}