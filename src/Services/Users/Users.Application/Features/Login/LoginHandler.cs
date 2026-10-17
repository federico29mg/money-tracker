using Users.Application.Abstractions;
using Users.Application.Common;

namespace Users.Application.Features.Login;

public sealed class LoginHandler(IIdentityService identityService, IAccessTokenIssuer accessTokenIssuer)
{
    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var userId = await identityService.ValidateCredentialsAsync(command.Email, command.Password, cancellationToken)
                     ?? throw new InvalidCredentialsException();

        var accessToken = accessTokenIssuer.Issue(userId, command.Email);

        return new LoginResponse(accessToken.Value, accessToken.ExpiresAtUtc);
    }
}