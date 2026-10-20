using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasConversion(id => id.Value, value => CategoryId.From(value))
            .ValueGeneratedNever();

        builder.Property(category => category.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value));

        builder.Property(category => category.Name)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        builder.Property(category => category.Type)
            .HasConversion<string>()
            .HasMaxLength(ColumnTypes.EnumLength);

        // The database's default collation is case-insensitive, so "Gym" and "gym" clash here too.
        builder.HasIndex(category => new { category.UserId, category.Type, category.Name })
            .IsUnique();
    }
}