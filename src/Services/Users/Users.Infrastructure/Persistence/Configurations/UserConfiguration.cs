using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Users.Domain;

namespace Users.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .ValueGeneratedNever();

        builder.Property(user => user.DisplayName)
            .HasMaxLength(User.MaxDisplayNameLength)
            .IsRequired();

        builder.ComplexProperty(user => user.Settings, settings =>
            settings.Property(s => s.DefaultCurrency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasColumnName("DefaultCurrency")
                .HasMaxLength(3)
                .IsRequired());
    }
}