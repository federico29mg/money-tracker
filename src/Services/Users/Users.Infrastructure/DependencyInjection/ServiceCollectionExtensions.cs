using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Users.Application.Abstractions;
using Users.Infrastructure.Authentication;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Persistence.Repositories;

namespace Users.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("UsersDb")
                               ?? throw new InvalidOperationException("Connection string 'UsersDb' is not configured.");

        services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UsersDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddUsersIdentity();
        services.AddUsersJwt(configuration);

        return services;
    }

    private static void AddUsersIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<UsersDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();
    }

    private static void AddUsersJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => JwtSigningKey.IsValidPrivateKey(options.SigningKey),
                $"{JwtOptions.SectionName}:SigningKey must be a base64-encoded PKCS#8 RSA private key of at least 2048 bits.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<JwtSigningKey>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
    }
}