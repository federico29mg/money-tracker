using Microsoft.Extensions.DependencyInjection;

using Transactions.Application.Features.Accounts.ArchiveAccount;
using Transactions.Application.Features.Accounts.GetAccount;
using Transactions.Application.Features.Accounts.GetAccounts;
using Transactions.Application.Features.Accounts.OpenAccount;
using Transactions.Application.Features.Accounts.RenameAccount;
using Transactions.Application.Features.Categories.ArchiveCategory;
using Transactions.Application.Features.Categories.CreateCategory;
using Transactions.Application.Features.Categories.GetCategories;
using Transactions.Application.Features.Categories.GetCategory;
using Transactions.Application.Features.Categories.RenameCategory;

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
        services.AddScoped<CreateCategoryHandler>();
        services.AddScoped<RenameCategoryHandler>();
        services.AddScoped<ArchiveCategoryHandler>();
        services.AddScoped<GetCategoryHandler>();
        services.AddScoped<GetCategoriesHandler>();

        return services;
    }
}