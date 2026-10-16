using Microsoft.EntityFrameworkCore;

using Users.Application.Abstractions;
using Users.Domain.UserAggregate;

namespace Users.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken)
        => dbContext.Set<User>().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public void Add(User user) => dbContext.Set<User>().Add(user);
}