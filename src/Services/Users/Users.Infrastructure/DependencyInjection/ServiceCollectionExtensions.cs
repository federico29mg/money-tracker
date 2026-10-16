using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Users.Application.Abstractions;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Persistence.Repositories;

namespace Users.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UsersDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}