using Transactions.Application.Abstractions;
using Transactions.Application.Common;
using Transactions.Application.Features.Accounts;
using Transactions.Application.Features.Categories;
using Transactions.Domain.Common;
using Transactions.Domain.TransactionAggregate;

namespace Transactions.Application.Features.Transactions.RecordTransaction;

public sealed class RecordTransactionHandler(
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(RecordTransactionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var account = await accountRepository.GetRequiredAsync(command.AccountId, command.UserId, cancellationToken);
        var category = command.CategoryId is { } categoryId
            ? await categoryRepository.GetRequiredAsync(categoryId, command.UserId, cancellationToken)
            : await categoryRepository.GetDefaultAsync(UserId.From(command.UserId), command.Type, cancellationToken)
              ?? throw new ValidationException(new Dictionary<string, string[]>
              {
                  [nameof(command.CategoryId)] = [$"There's no default {command.Type} category; choose a category."]
              });

        var currency = command.CurrencyCode is null ? account.Currency : Currency.FromCode(command.CurrencyCode);

        var transaction = Transaction.Record(
            account,
            category,
            command.Type,
            Money.Of(command.Amount, currency),
            command.Date,
            command.Name,
            command.Description);

        transactionRepository.Add(transaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.Id.Value;
    }
}