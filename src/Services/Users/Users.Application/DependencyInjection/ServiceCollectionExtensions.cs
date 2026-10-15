using Microsoft.Extensions.DependencyInjection;

using Users.Application.Features.ChangeDefaultCurrency;
using Users.Application.Features.GetUser;
using Users.Application.Features.RegisterUser;

namespace Users.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<ChangeDefaultCurrencyHandler>();
        
        return services;
    }
}