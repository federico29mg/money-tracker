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
using Transactions.Application.Features.Provisioning.ProvisionUser;
using Transactions.Application.Features.Transactions.GetPendingTransactions;
using Transactions.Application.Features.Transactions.GetTransaction;
using Transactions.Application.Features.Transactions.GetTransactions;
using Transactions.Application.Features.Transactions.RecordTransaction;
using Transactions.Application.Features.Transactions.SettleTransactions;
using Transactions.Application.Features.Transfers.GetTransfer;
using Transactions.Application.Features.Transfers.GetTransfers;
using Transactions.Application.Features.Transfers.PayCreditCard;
using Transactions.Application.Features.Transfers.RecordTransfer;

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
        services.AddScoped<RecordTransactionHandler>();
        services.AddScoped<GetTransactionHandler>();
        services.AddScoped<GetTransactionsHandler>();
        services.AddScoped<GetPendingTransactionsHandler>();
        services.AddScoped<SettleTransactionsHandler>();
        services.AddScoped<RecordTransferHandler>();
        services.AddScoped<GetTransferHandler>();
        services.AddScoped<GetTransfersHandler>();
        services.AddScoped<PayCreditCardHandler>();
        services.AddScoped<ProvisionUserHandler>();

        return services;
    }
}