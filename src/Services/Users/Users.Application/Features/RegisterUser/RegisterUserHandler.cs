using Users.Application.Abstractions;
using Users.Domain;

namespace Users.Application.Features.RegisterUser;

public sealed class RegisterUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var settings = command.DefaultCurrencyCode is null
            ? UserSettings.Default
            : UserSettings.Create(Currency.FromCode(command.DefaultCurrencyCode));

        var user = User.Register(UserId.New(), command.DisplayName, settings);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id.Value;
    }
}