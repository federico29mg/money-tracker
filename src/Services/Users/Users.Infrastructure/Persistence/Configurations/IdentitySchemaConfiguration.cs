using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Users.Infrastructure.Identity;

namespace Users.Infrastructure.Persistence.Configurations;

internal static class IdentitySchemaConfiguration
{
    public const string Schema = "identity";

    public static void ApplyIdentitySchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers", Schema);
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", Schema);
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", Schema);
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", Schema);
    }
}