using Users.Application.Abstractions;
using Users.Domain.Common;
using Users.Domain.UserAggregate;

namespace Users.Application.Features.RegisterUser;

public sealed class RegisterUserHandler(
    IUserRepository userRepository,
    IIdentityService identityService,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var settings = command.DefaultCurrencyCode is null
            ? UserSettings.Default
            : UserSettings.Create(Currency.FromCode(command.DefaultCurrencyCode));

        var user = User.Register(UserId.New(), command.DisplayName, settings);

        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await identityService.CreateUserAsync(user.Id, command.Email, command.Password, token);

            userRepository.Add(user);
            await unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);

        return user.Id.Value;
    }
}