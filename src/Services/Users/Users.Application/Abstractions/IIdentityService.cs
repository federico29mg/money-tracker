using Users.Domain.UserAggregate;

namespace Users.Application.Abstractions;

public interface IIdentityService
{
    Task CreateUserAsync(UserId userId, string email, string password, CancellationToken cancellationToken);

    Task<UserId?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}