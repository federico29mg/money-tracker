using Microsoft.Extensions.DependencyInjection;

using Transactions.Application.Features.Accounts.ArchiveAccount;
using Transactions.Application.Features.Accounts.GetAccount;
using Transactions.Application.Features.Accounts.GetAccounts;
using Transactions.Application.Features.Accounts.OpenAccount;
using Transactions.Application.Features.Accounts.RenameAccount;

namespace Transactions.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionsApplication(this IServiceCollection services)
    {
        services.AddScoped<OpenAccountHandler>();
        services.AddScoped<RenameAccountHandler>();
        services.AddScoped<ArchiveAccountHandler>();
        services.AddScoped<GetAccountHandler>();
        services.AddScoped<GetAccountsHandler>();

        return services;
    }
}