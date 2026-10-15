using Users.Application.Abstractions;
using Users.Application.Common;
using Users.Domain;

namespace Users.Application.Features.ChangeDefaultCurrency;

public sealed class ChangeDefaultCurrencyHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(ChangeDefaultCurrencyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var user = await userRepository.GetByIdAsync(UserId.From(command.UserId), cancellationToken) 
                   ?? throw new NotFoundException($"User '{command.UserId}' was not found.");
        
        user.ChangeDefaultCurrency(Currency.FromCode(command.CurrencyCode));
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}