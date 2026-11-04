using Azure.Messaging.ServiceBus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Transactions.Application.Abstractions;
using Transactions.Infrastructure.Messaging;
using Transactions.Infrastructure.Persistence;
using Transactions.Infrastructure.Persistence.Repositories;

namespace Transactions.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("TransactionsDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'TransactionsDb' is not configured.");

        services.AddDbContext<TransactionsDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TransactionsDbContext>());

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();

        services.AddTransactionsMessaging(configuration);

        return services;
    }

    private static void AddTransactionsMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ServiceBus")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'ServiceBus' is not configured.");

        services
            .AddOptions<ServiceBusOptions>()
            .Bind(configuration.GetSection(ServiceBusOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(_ => new ServiceBusClient(connectionString));
        services.AddHostedService<UserEventsConsumer>();
    }
}