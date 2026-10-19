using Transactions.Application.Abstractions;

namespace Transactions.Application.Features.Accounts.RenameAccount;

public sealed class RenameAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(RenameAccountCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var account = await accountRepository.GetRequiredAsync(command.AccountId, command.UserId, cancellationToken);

        account.Rename(command.Name);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}