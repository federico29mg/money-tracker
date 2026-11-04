using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Provisioning.ProvisionUser;

// Gives a new user what they need to start: a Cash account and a default category per type.
// Safe to run more than once: it only adds what's missing, so a repeated message changes nothing.
public sealed class ProvisionUserHandler(
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
{
    public const string CashAccountName = "Cash";

    public async Task HandleAsync(ProvisionUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var userId = UserId.From(command.UserId);
        var currency = Currency.FromCode(command.CurrencyCode);

        // Archived ones count too: a user who archived their Cash account doesn't get it back.
        var accounts = await accountRepository.ListAsync(userId, includeArchived: true, cancellationToken);

        if (!accounts.Any(account => account.Type == AccountType.Cash && account.Currency == currency))
        {
            accountRepository.Add(
                Account.Open(userId, CashAccountName, AccountType.Cash, Money.Zero(currency)));
        }

        var categories =
            await categoryRepository.ListAsync(userId, type: null, includeArchived: true, cancellationToken);

        foreach (var type in Enum.GetValues<TransactionType>())
        {
            var ofType = categories.Where(category => category.Type == type).ToList();

            if (ofType.Any(category => category.IsDefault))
            {
                continue;
            }

            // Category names are unique per type, so an existing "Uncategorized" becomes the default.
            var sameName = ofType.FirstOrDefault(category =>
                string.Equals(category.Name, Category.DefaultName, StringComparison.OrdinalIgnoreCase));

            if (sameName is not null)
            {
                sameName.MakeDefault();
            }
            else
            {
                categoryRepository.Add(Category.CreateDefault(userId, type));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}