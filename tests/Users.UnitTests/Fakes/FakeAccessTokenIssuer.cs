using Users.Application.Abstractions;
using Users.Domain.UserAggregate;

namespace Users.UnitTests.Fakes;

public sealed class FakeAccessTokenIssuer : IAccessTokenIssuer
{
    public static readonly DateTimeOffset ExpiresAtUtc = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public AccessToken Issue(UserId userId, string email) => new($"token-for-{userId}", ExpiresAtUtc);
}