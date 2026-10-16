using Users.Domain.UserAggregate;

namespace Users.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken);
    void Add(User user);
}