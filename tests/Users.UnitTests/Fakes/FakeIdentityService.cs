using Users.Application.Abstractions;
using Users.Application.Common;
using Users.Domain.UserAggregate;

namespace Users.UnitTests.Fakes;

public sealed class FakeIdentityService : IIdentityService
{
    private readonly Dictionary<string, (UserId UserId, string Password)> _accounts =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, (UserId UserId, string Password)> Accounts => _accounts;

    public Task CreateUserAsync(UserId userId, string email, string password, CancellationToken cancellationToken)
    {
        if (!_accounts.TryAdd(email, (userId, password)))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<UserId?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken) =>
        Task.FromResult(_accounts.TryGetValue(email, out var account) && account.Password == password
            ? account.UserId
            : (UserId?)null);
}