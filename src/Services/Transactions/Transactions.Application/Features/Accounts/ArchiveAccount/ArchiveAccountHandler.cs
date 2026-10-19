using Transactions.Application.Abstractions;

namespace Transactions.Application.Features.Accounts.ArchiveAccount;

public sealed class ArchiveAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(ArchiveAccountCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var account = await accountRepository.GetRequiredAsync(command.AccountId, command.UserId, cancellationToken);

        account.Archive();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}