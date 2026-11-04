using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Users.Infrastructure.Messaging;

namespace Users.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasMaxLength(OutboxMessage.MaxTypeLength)
            .IsRequired();

        builder.Property(message => message.Content)
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasMaxLength(OutboxMessage.MaxErrorLength);

        // The publisher only ever looks for unsent messages, oldest first; sent ones stay out of the index.
        builder.HasIndex(message => message.OccurredAt)
            .HasFilter("[ProcessedAt] IS NULL");
    }
}