using Users.Application.Abstractions;
using Users.Domain;

namespace Users.UnitTests.Fakes;

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<UserId, User> _users = [];
    
    public IReadOnlyCollection<User> StoredUsers => _users.Values;
    
    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken) 
        => Task.FromResult(_users.GetValueOrDefault(id));
    
    public void Add(User user) => _users.Add(user.Id, user);
}