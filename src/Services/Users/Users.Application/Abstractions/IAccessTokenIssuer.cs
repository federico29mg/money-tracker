using Users.Domain.UserAggregate;

namespace Users.Application.Abstractions;

public interface IAccessTokenIssuer
{
    AccessToken Issue(UserId userId, string email);
}