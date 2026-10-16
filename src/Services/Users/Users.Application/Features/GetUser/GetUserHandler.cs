using Users.Application.Abstractions;
using Users.Application.Common;
using Users.Domain.UserAggregate;

namespace Users.Application.Features.GetUser;

public sealed class GetUserHandler(IUserRepository userRepository)
{
    public async Task<UserResponse> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await userRepository.GetByIdAsync(UserId.From(query.UserId), cancellationToken)
                   ?? throw new NotFoundException($"User '{query.UserId}' was not found.");

        return new UserResponse(user.Id.Value, user.DisplayName, user.Settings.DefaultCurrency.Code);
    }
}