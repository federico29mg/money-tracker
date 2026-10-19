using Transactions.Application.Abstractions;
using Transactions.Domain.AccountAggregate;
using Transactions.Domain.Common;

namespace Transactions.Application.Features.Accounts.OpenAccount;

public sealed class OpenAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(OpenAccountCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var openingBalance = Money.Of(command.OpeningBalance, Currency.FromCode(command.CurrencyCode));
        var account = Account.Open(UserId.From(command.UserId), command.Name, command.Type, openingBalance);

        accountRepository.Add(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return account.Id.Value;
    }
}